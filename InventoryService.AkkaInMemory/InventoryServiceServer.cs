using Akka.Actor;
using InventoryService.InMemory;
using InventoryService.Messages;
using InventoryService.Messages.Models;
using InventoryService.Messages.Request;
using InventoryService.Server;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace InventoryService.AkkaInMemoryServer
{
    public class InventoryServiceServer : IInventoryServiceDirect, IDisposable
    {
        private ActorSystem Sys { set; get; }

        private InventoryServerOptions Options { set; get; }

        public InventoryServiceServer(InventoryServerOptions options = null)
        {
            Options = options ?? new InventoryServerOptions();
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

        private void InitializeWithInventorydata(InventoryServerOptions options)
        {
            Task.Run(async () =>
            {
             await    UpdateQuantityAsync(options.InitialInventory, options.InitialInventory.Quantity);//.TODO /* USE PROPER ASYNC AWAIT HERE */
          await  ReserveAsync(options.InitialInventory, options.InitialInventory.Reserved);//.TODO /* USE PROPER ASYNC AWAIT HERE */
              await  PlaceHoldAsync(options.InitialInventory, options.InitialInventory.Holds);//.TODO /* USE PROPER ASYNC AWAIT HERE */
                var result = GetInventoryAsync(options.InitialInventory.ProductId).Result;
            if (!result.Successful ||
                result.RealTimeInventory == null ||
                result.RealTimeInventory.ProductId != options.InitialInventory.ProductId ||
                result.RealTimeInventory.Quantity != options.InitialInventory.Quantity ||
                result.RealTimeInventory.Reserved != options.InitialInventory.Reserved ||
                result.RealTimeInventory.Holds != options.InitialInventory.Holds)
            {
                throw new Exception("Error initializing data into remote inventory actor ");
            }

            }).Wait();
           
        }

        public IActorRef inventoryActor { get; set; }

        public static TimeSpan GENERAL_WAIT_TIME = TimeSpan.FromSeconds(500000);

        public InventoryServiceApplication InventoryServiceApplication { get; set; }

        public async Task<IInventoryServiceCompletedMessage> UpdateQuantityAsync(RealTimeInventory product, int quantity)
        {
            return await inventoryActor.Ask<IInventoryServiceCompletedMessage>(new UpdateQuantityMessage(product.ProductId, quantity), GENERAL_WAIT_TIME);
        }

        public async Task<IInventoryServiceCompletedMessage> ReserveAsync(RealTimeInventory product, int reserveQuantity)
        {
            return await inventoryActor.Ask<IInventoryServiceCompletedMessage>(new ReserveMessage(product.ProductId, reserveQuantity), GENERAL_WAIT_TIME);
        }

        public async Task<IInventoryServiceCompletedMessage> PurchaseAsync(RealTimeInventory product, int purchaseQuantity)
        {
            return await inventoryActor.Ask<IInventoryServiceCompletedMessage>(new PurchaseMessage(product.ProductId, purchaseQuantity), GENERAL_WAIT_TIME);
        }

        public async Task<IInventoryServiceCompletedMessage> PlaceHoldAsync(RealTimeInventory product, int holdQuantity)
        {
            return await inventoryActor.Ask<IInventoryServiceCompletedMessage>(new PlaceHoldMessage(product.ProductId, holdQuantity), GENERAL_WAIT_TIME);
        }

        public async Task<IInventoryServiceCompletedMessage> UpdateQuantityAndHoldAsync(RealTimeInventory product, int holdQuantity)
        {
            return await inventoryActor.Ask<IInventoryServiceCompletedMessage>(new UpdateAndHoldQuantityMessage(product.ProductId, holdQuantity), GENERAL_WAIT_TIME);
        }

        public async Task<IInventoryServiceCompletedMessage> PurchaseFromHoldsAsync(RealTimeInventory product, int purchaseQuantity)
        {
            return await inventoryActor.Ask<IInventoryServiceCompletedMessage>(new PurchaseFromHoldsMessage(product.ProductId, purchaseQuantity), GENERAL_WAIT_TIME);
        }

        public async Task<IInventoryServiceCompletedMessage> GetInventoryAsync(string inventoryName)
        {
            return await inventoryActor.Ask<IInventoryServiceCompletedMessage>(new GetInventoryMessage(inventoryName), GENERAL_WAIT_TIME);
        }

        public async Task<IInventoryServiceCompletedMessage> ReserveAsync(ActorSelection inventoryActorSelection, int reserveQuantity, string productId = "product1")
        {
            return await inventoryActor.Ask<IInventoryServiceCompletedMessage>(new ReserveMessage(productId, reserveQuantity), GENERAL_WAIT_TIME);
        }

        public void Dispose()
        {
            InventoryServiceApplication.Stop();
            Sys?.Terminate();
            Sys.AwaitTermination();
            Sys?.Dispose();
        }
    }
}