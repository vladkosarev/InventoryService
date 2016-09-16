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
        private IInventoryStorage InventoryStorage { set; get; }
        private ActorSystem Sys { set; get; }

        public InventoryServiceServer(RealTimeInventory product = null, ActorSystem sys = null)
        {
            Sys = sys;
            InventoryStorage = new Storage.InMemoryLib.InMemory();
            InitializeAndGetInventoryActor(product);
        }

        public InventoryServiceServer(IInventoryStorage inventoryStorage, RealTimeInventory product = null, ActorSystem sys = null)
        {
            Sys = sys;
            InventoryStorage = inventoryStorage;
            InitializeAndGetInventoryActor(product);
        }

        public IActorRef inventoryActor { get; set; }

        public static TimeSpan GENERAL_WAIT_TIME = TimeSpan.FromSeconds(500000);

        private void InitializeAndGetInventoryActor(RealTimeInventory product = null)
        {
            InventoryServiceApplication = new InventoryServiceApplication();
            var address = ConfigurationManager.AppSettings["RemoteActorAddress"];
            InventoryServiceApplication.Start();

            Sys = Sys ?? InventoryServiceApplication.InventoryServiceServerApp.ActorSystem;
            inventoryActor = Sys.ActorSelection(address).ResolveOne(TimeSpan.FromSeconds(3)).Result;

            if (product != null)
            {
                UpdateQuantityAsync(product, product.Quantity).Wait();
                ReserveAsync(product, product.Reserved).Wait();
                PlaceHoldAsync(product, product.Holds).Wait();
                var result = GetInventoryAsync(product.ProductId).Result;
                if (!result.Successful ||
                    result.RealTimeInventory == null ||
                    result.RealTimeInventory.ProductId != product.ProductId ||
                    result.RealTimeInventory.Quantity != product.Quantity ||
                    result.RealTimeInventory.Reserved != product.Reserved ||
                    result.RealTimeInventory.Holds != product.Holds)
                {
                    throw new Exception("Error initializing data into remote inventory actor ");
                }
            }
        }

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
            Sys?.Terminate().Wait();
            Sys?.Dispose();
            InventoryStorage?.FlushAsync();
            InventoryStorage?.Dispose();
        }
    }
}