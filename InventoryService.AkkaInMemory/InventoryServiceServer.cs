using Akka.Actor;
using InventoryService.InMemory;
using InventoryService.Messages;
using InventoryService.Messages.Models;
using InventoryService.Messages.Request;
using InventoryService.Server;
using InventoryService.ServiceClientDeployment;
using InventoryService.Storage;
using Microsoft.Owin.Hosting;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace InventoryService.AkkaInMemoryServer
{
    public class InventoryServiceServer : IInventoryServiceDirect, IDisposable
    {
        private ActorSystem Sys { set; get; }
        private IInventoryStorage TestInventoryStorage { set; get; }
        private InventoryServerOptions Options { set; get; }
        private bool DontUseActorSystem { set; get; }

        public InventoryServiceServer(IPerformanceService performanceService, InventoryServerOptions options = null)
        {
            Options = options ?? new InventoryServerOptions();
            if (Options.DontUseActorSystem)
            {
                DontUseActorSystem = Options.DontUseActorSystem;
                TestInventoryStorage = Options.StorageType != null
                    ? (IInventoryStorage)Activator.CreateInstance(Options.StorageType)
                    : new Storage.InMemoryLib.InMemory();
                if (Options.InitialInventory != null)
                {
                    TestInventoryStorage.WriteInventoryAsync(Options.InitialInventory);
                }
            }
            else
            {
                Sys = Options.ClientActorSystem;
                InventoryServiceApplication = new InventoryServiceApplication();

                if (string.IsNullOrEmpty(Options.InventoryActorAddress))
                {
                    Options.InventoryActorAddress = ConfigurationManager.AppSettings["RemoteInventoryActorAddress"];
                }

                InventoryServiceApplication.Start(performanceService, Options.OnInventoryActorSystemReady, Options.StorageType,
                    serverEndPoint: Options.ServerEndPoint, serverActorSystemName: Options.ServerActorSystemName,
                    serverActorSystem: Options.ServerActorSystem,
                    serverActorSystemConfig: Options.ServerActorSystemConfig);

                Sys = Sys ?? InventoryServiceApplication.InventoryServiceServerApp.ActorSystem;
                var selection = Sys.ActorSelection(Options.InventoryActorAddress);

                inventoryActor = TryResolveActorSelection(selection);

                var serverEndPoint = Options.ServerEndPoint;//ConfigurationManager.AppSettings["ServerEndPoint"];
                if (!string.IsNullOrEmpty(serverEndPoint))
                {
                    OwinRef = WebApp.Start<Startup>(url: serverEndPoint);
                }

                if (Options.InitialInventory != null)
                {
                    InitializeWithInventorydata(Options);
                }
            }
        }

        public IDisposable OwinRef { get; set; }

        private static IActorRef TryResolveActorSelection(ActorSelection selection)
        {
            return Task.Run(async () =>
            {
                var counter = 0;
                IActorRef result = null;
                var lastException = new Exception();
                while (result == null && counter < 3)
                {
                    try
                    {
                        var identity = await selection.Ask<ActorIdentity>(new Identify(null), TimeSpan.FromSeconds(30));
                        if (identity.Subject == null)
                        {
                            throw new Exception("Unable to obtain iactorref of address " + selection.PathString + " even after obtaining a response with identity " + identity + " with message ID : " + identity?.MessageId);
                        }
                        result = identity.Subject;
                    }
                    catch (Exception e)
                    {
                        lastException = e;
                        counter++;
                        await Task.Delay(TimeSpan.FromMilliseconds(500 * counter));
                    }
                }
                if (result == null)
                {
                    throw lastException;
                }
                return result;
            }).Result;
        }

        private void InitializeWithInventorydata(InventoryServerOptions options)
        {
            Task.Run(async () =>
       {
           await UpdateQuantityAsync(options.InitialInventory, Options.InitialInventory.Quantity);//.TODO /* USE PROPER ASYNC AWAIT HERE */
           await ReserveAsync(options.InitialInventory, Options.InitialInventory.Reserved);//.TODO /* USE PROPER ASYNC AWAIT HERE */
           await PlaceHoldAsync(options.InitialInventory, Options.InitialInventory.Holds);//.TODO /* USE PROPER ASYNC AWAIT HERE */
           var result = await GetInventoryAsync(Options.InitialInventory.ProductId);
           if (!result.Successful ||
               result.RealTimeInventory == null ||
               result.RealTimeInventory.ProductId != Options.InitialInventory.ProductId ||
               result.RealTimeInventory.Quantity != Options.InitialInventory.Quantity ||
               result.RealTimeInventory.Reserved != Options.InitialInventory.Reserved ||
               result.RealTimeInventory.Holds != Options.InitialInventory.Holds)
           {
               throw new Exception("Error initializing data into remote inventory actor ");
           }
       }).Wait();
        }

        public IActorRef inventoryActor { get; set; }

        public static TimeSpan GENERAL_WAIT_TIME = TimeSpan.FromSeconds(500000);

        public InventoryServiceApplication InventoryServiceApplication { get; set; }

        public async Task<IInventoryServiceCompletedMessage> PerformOperation(IRequestMessage request, Task<OperationResult<IRealTimeInventory>> response, IRealTimeInventory originalInventory)
        {
            var perf = new TestPerformanceService();
            perf.Init();
            return (await response).ProcessAndSendResult(request, CompletedMessageFactory.GetResponseCompletedMessage(request), originalInventory, null, perf).InventoryServiceCompletedMessage;
        }

        public Task<IInventoryServiceCompletedMessage> UpdateQuantityAsync(RealTimeInventory product, int quantity)
        {
            var request = new UpdateQuantityMessage(product.ProductId, quantity);

            if (DontUseActorSystem)
            {
                return PerformOperation(
                    request
                    , product.UpdateQuantityAsync(TestInventoryStorage, request.ProductId, request.Update),
                     TestInventoryStorage
                   .ReadInventoryAsync(request.ProductId)
                   .Result.Result);
            }

            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(request, GENERAL_WAIT_TIME);
        }

        public Task<IInventoryServiceCompletedMessage> ReserveAsync(RealTimeInventory product, int reserveQuantity)
        {
            var request = new ReserveMessage(product.ProductId, reserveQuantity);

            if (DontUseActorSystem)
            {
                return PerformOperation(request, product.ReserveAsync(TestInventoryStorage, request.ProductId, request.Update),
                    TestInventoryStorage
                  .ReadInventoryAsync(request.ProductId)
                  .Result.Result);
            }

            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(request, GENERAL_WAIT_TIME);
        }

        public Task<IInventoryServiceCompletedMessage> PurchaseAsync(RealTimeInventory product, int purchaseQuantity)
        {
            var request = new PurchaseMessage(product.ProductId, purchaseQuantity);

            if (DontUseActorSystem)
            {
                return PerformOperation(request, product.PurchaseAsync(TestInventoryStorage, request.ProductId, request.Update), TestInventoryStorage.ReadInventoryAsync(request.ProductId).Result.Result);
            }
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(request, GENERAL_WAIT_TIME);
        }

        public Task<IInventoryServiceCompletedMessage> ResetInventoryQuantityReserveAndHoldAsync(
            RealTimeInventory product,
            int quantity,
            int reserveQuantity,
            int holdsQuantity)
        {
            var request = new ResetInventoryQuantityReserveAndHoldMessage(product.ProductId, quantity, reserveQuantity, holdsQuantity);

            if (DontUseActorSystem)
            {
                return PerformOperation(request, product.ResetInventoryQuantityReserveAndHoldAsync(TestInventoryStorage, request.ProductId, request.Update, request.Reservations, request.Holds),
                    TestInventoryStorage
                  .ReadInventoryAsync(request.ProductId)
                  .Result.Result);
            }
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(request, GENERAL_WAIT_TIME);
        }

        public Task<IInventoryServiceCompletedMessage> PlaceHoldAsync(RealTimeInventory product, int holdQuantity)
        {
            var request = new PlaceHoldMessage(product.ProductId, holdQuantity);

            if (DontUseActorSystem)
            {
                return PerformOperation(request, product.PlaceHoldAsync(TestInventoryStorage, request.ProductId, request.Update),
                    TestInventoryStorage
                  .ReadInventoryAsync(request.ProductId)
                  .Result.Result);
            }
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(request, GENERAL_WAIT_TIME);
        }

        public Task<IInventoryServiceCompletedMessage> UpdateQuantityAndHoldAsync(RealTimeInventory product, int holdQuantity)
        {
            var request = new UpdateAndHoldQuantityMessage(product.ProductId, holdQuantity);

            if (DontUseActorSystem)
            {
                return PerformOperation(request, product.UpdateQuantityAndHoldAsync(TestInventoryStorage, request.ProductId, request.Update),
                    TestInventoryStorage
                  .ReadInventoryAsync(request.ProductId)
                  .Result.Result);
            }
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(request, GENERAL_WAIT_TIME);
        }

        public Task<IInventoryServiceCompletedMessage> PurchaseFromHoldsAsync(RealTimeInventory product, int purchaseQuantity)
        {
            var request = new PurchaseFromHoldsMessage(product.ProductId, purchaseQuantity);

            if (DontUseActorSystem)
            {
                return PerformOperation(request, product.PurchaseFromHoldsAsync(TestInventoryStorage, request.ProductId, request.Update),
                    TestInventoryStorage
                  .ReadInventoryAsync(request.ProductId)
                  .Result.Result);
            }
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(request, GENERAL_WAIT_TIME);
        }

        public Task<IInventoryServiceCompletedMessage> GetInventoryAsync(string inventoryName)
        {
            var request = new GetInventoryMessage(inventoryName);

            if (DontUseActorSystem)
            {
                return PerformOperation(request, Options.InitialInventory.ReadInventoryFromStorageAsync(TestInventoryStorage, request.ProductId),
                    TestInventoryStorage
                  .ReadInventoryAsync(request.ProductId)
                  .Result.Result);
            }
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(request, GENERAL_WAIT_TIME);
        }

        public Task<IInventoryServiceCompletedMessage> ReserveAsync(ActorSelection inventoryActorSelection, int reserveQuantity, string productId = "product1")
        {
            var request = new ReserveMessage(productId, reserveQuantity);

            if (DontUseActorSystem)
            {
                return PerformOperation(request, Options.InitialInventory.ReserveAsync(TestInventoryStorage, request.ProductId, request.Update),
                    TestInventoryStorage
                  .ReadInventoryAsync(request.ProductId)
                  .Result.Result);
            }
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(request, GENERAL_WAIT_TIME);
        }

        public void Dispose()
        {
            OwinRef?.Dispose();
            InventoryServiceApplication?.Stop();
            Sys?.Terminate().Wait();
            Sys?.Dispose();
        }
    }
}