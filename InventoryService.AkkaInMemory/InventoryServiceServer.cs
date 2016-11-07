using Akka.Actor;
using InventoryService.InMemory;
using InventoryService.Messages;
using InventoryService.Messages.Models;
using InventoryService.Messages.Request;
using InventoryService.Server;
using InventoryService.Storage;
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

        public InventoryServiceServer(InventoryServerOptions options = null)
        {
            Options = options ?? new InventoryServerOptions();
            if (Options.DontUseActorSystem)
            {
                DontUseActorSystem = Options.DontUseActorSystem;
                TestInventoryStorage = Options.StorageType != null ? (IInventoryStorage)Activator.CreateInstance(Options.StorageType) : new Storage.InMemoryLib.InMemory();
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
                    Options.InventoryActorAddress = ConfigurationManager.AppSettings["RemoteActorAddress"];
                }

                InventoryServiceApplication.Start(Options.OnInventoryActorSystemReady, Options.StorageType, serverEndPoint: Options.ServerEndPoint, serverActorSystemName: Options.ServerActorSystemName, serverActorSystem: Options.ServerActorSystem, serverActorSystemConfig: Options.ServerActorSystemConfig);

                Sys = Sys ?? InventoryServiceApplication.InventoryServiceServerApp.ActorSystem;
                inventoryActor = Sys.ActorSelection(Options.InventoryActorAddress).ResolveOne(TimeSpan.FromSeconds(3)).Result;

                if (Options.InitialInventory != null)
                {
                    InitializeWithInventorydata(Options);
                }
            }
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
            return (await response).ProcessAndSendResult(request, CompletedMessageFactory.GetResponseCompletedMessage(request), originalInventory,null).InventoryServiceCompletedMessage;
        }

        public async Task<IInventoryServiceCompletedMessage> UpdateQuantityAsync(RealTimeInventory product, int quantity)
        {
            var request = new UpdateQuantityMessage(product.ProductId, quantity);

            if (DontUseActorSystem)
            {
                return await PerformOperation(
                    request
                    , product.UpdateQuantityAsync(TestInventoryStorage, request.ProductId, request.Update),
                     TestInventoryStorage
                   .ReadInventoryAsync(request.ProductId)
                   .Result.Result);
            }

            return await inventoryActor.Ask<IInventoryServiceCompletedMessage>(request, GENERAL_WAIT_TIME);
        }

        public async Task<IInventoryServiceCompletedMessage> ReserveAsync(RealTimeInventory product, int reserveQuantity)
        {
            var request = new ReserveMessage(product.ProductId, reserveQuantity);

            if (DontUseActorSystem)
            {
                return await PerformOperation(request, product.ReserveAsync(TestInventoryStorage, request.ProductId, request.Update),
                    TestInventoryStorage
                  .ReadInventoryAsync(request.ProductId)
                  .Result.Result);
            }

            return await inventoryActor.Ask<IInventoryServiceCompletedMessage>(request, GENERAL_WAIT_TIME);
        }

        public async Task<IInventoryServiceCompletedMessage> PurchaseAsync(RealTimeInventory product, int purchaseQuantity)
        {
            var request = new PurchaseMessage(product.ProductId, purchaseQuantity);

            if (DontUseActorSystem)
            {
                return await PerformOperation(request, product.PurchaseAsync(TestInventoryStorage, request.ProductId, request.Update),TestInventoryStorage.ReadInventoryAsync(request.ProductId).Result.Result);
            }
            return await inventoryActor.Ask<IInventoryServiceCompletedMessage>(request, GENERAL_WAIT_TIME);
        }

        public async Task<IInventoryServiceCompletedMessage> ResetInventoryQuantityReserveAndHoldAsync(
            RealTimeInventory product,
            int quantity,
            int reserveQuantity,
            int holdsQuantity)
        {
            var request = new ResetInventoryQuantityReserveAndHoldMessage(product.ProductId, quantity, reserveQuantity, holdsQuantity);

            if (DontUseActorSystem)
            {
                return await PerformOperation(request, product.ResetInventoryQuantityReserveAndHoldAsync(TestInventoryStorage, request.ProductId, request.Update, request.Reservations, request.Holds),
                    TestInventoryStorage
                  .ReadInventoryAsync(request.ProductId)
                  .Result.Result);
            }
            return await inventoryActor.Ask<IInventoryServiceCompletedMessage>(request, GENERAL_WAIT_TIME);
        }

        public async Task<IInventoryServiceCompletedMessage> PlaceHoldAsync(RealTimeInventory product, int holdQuantity)
        {
            var request = new PlaceHoldMessage(product.ProductId, holdQuantity);

            if (DontUseActorSystem)
            {
                return await PerformOperation(request, product.PlaceHoldAsync(TestInventoryStorage, request.ProductId, request.Update),
                    TestInventoryStorage
                  .ReadInventoryAsync(request.ProductId)
                  .Result.Result);
            }
            return await inventoryActor.Ask<IInventoryServiceCompletedMessage>(request, GENERAL_WAIT_TIME);
        }

        public async Task<IInventoryServiceCompletedMessage> UpdateQuantityAndHoldAsync(RealTimeInventory product, int holdQuantity)
        {
            var request = new UpdateAndHoldQuantityMessage(product.ProductId, holdQuantity);

            if (DontUseActorSystem)
            {
                return await PerformOperation(request, product.UpdateQuantityAndHoldAsync(TestInventoryStorage, request.ProductId, request.Update),
                    TestInventoryStorage
                  .ReadInventoryAsync(request.ProductId)
                  .Result.Result);
            }
            return await inventoryActor.Ask<IInventoryServiceCompletedMessage>(request, GENERAL_WAIT_TIME);
        }

        public async Task<IInventoryServiceCompletedMessage> PurchaseFromHoldsAsync(RealTimeInventory product, int purchaseQuantity)
        {
            var request = new PurchaseFromHoldsMessage(product.ProductId, purchaseQuantity);

            if (DontUseActorSystem)
            {
                return await PerformOperation(request, product.PurchaseFromHoldsAsync(TestInventoryStorage, request.ProductId, request.Update),
                    TestInventoryStorage
                  .ReadInventoryAsync(request.ProductId)
                  .Result.Result);
            }
            return await inventoryActor.Ask<IInventoryServiceCompletedMessage>(request, GENERAL_WAIT_TIME);
        }

        public async Task<IInventoryServiceCompletedMessage> GetInventoryAsync(string inventoryName)
        {
            var request = new GetInventoryMessage(inventoryName);

            if (DontUseActorSystem)
            {
                return await PerformOperation(request, Options.InitialInventory.ReadInventoryFromStorageAsync(TestInventoryStorage, request.ProductId),
                    TestInventoryStorage
                  .ReadInventoryAsync(request.ProductId)
                  .Result.Result);
            }
            return await inventoryActor.Ask<IInventoryServiceCompletedMessage>(request, GENERAL_WAIT_TIME);
        }

        public async Task<IInventoryServiceCompletedMessage> ReserveAsync(ActorSelection inventoryActorSelection, int reserveQuantity, string productId = "product1")
        {
            var request = new ReserveMessage(productId, reserveQuantity);

            if (DontUseActorSystem)
            {
                return await PerformOperation(request, Options.InitialInventory.ReserveAsync(TestInventoryStorage, request.ProductId, request.Update),
                    TestInventoryStorage
                  .ReadInventoryAsync(request.ProductId)
                  .Result.Result);
            }
            return await inventoryActor.Ask<IInventoryServiceCompletedMessage>(request, GENERAL_WAIT_TIME);
        }

        public void Dispose()
        {
            //InventoryServiceApplication?.Stop();
            //Sys?.Terminate().RunSynchronously();
            //Sys?.Dispose();
        }
    }
}