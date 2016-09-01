using Akka.Actor;
using InventoryService.Actors;
using InventoryService.Messages.Models;
using InventoryService.Messages.Request;
using InventoryService.Messages.Response;
using InventoryService.Storage;
using System;
using System.Threading.Tasks;

namespace InventoryService.Tests
{
    public class TestHelper
    {
        private IInventoryStorage InventoryService { set; get; }

        public TestHelper(IInventoryStorage inventoryService)
        {
            InventoryService = inventoryService;
        }

        public static TimeSpan GENERAL_WAIT_TIME = TimeSpan.FromSeconds(300);

        public IActorRef InitializeAndGetInventoryActor(RealTimeInventory product, ActorSystem sys)
        {
            var result = InventoryService.WriteInventoryAsync(new RealTimeInventory(product.ProductId, product.Quantity, product.Reserved, product.Holds));
            Task.WaitAll(result);

            var inventoryActor = sys.ActorOf(Props.Create(() => new InventoryActor(InventoryService, new TestPerformanceService(), true)));
            return inventoryActor;
        }

        public IInventoryServiceCompletedMessage UpdateQuantity(IActorRef inventoryActor, int quantity, string productId = "product1")
        {
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(new UpdateQuantityMessage(productId, quantity), GENERAL_WAIT_TIME).WaitAndGetOperationResult();
        }

        public IInventoryServiceCompletedMessage Reserve(IActorRef inventoryActor, int reserveQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(new ReserveMessage(productId, reserveQuantity), GENERAL_WAIT_TIME).WaitAndGetOperationResult();
        }

        public IInventoryServiceCompletedMessage Purchase(IActorRef inventoryActor, int purchaseQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(new PurchaseMessage(productId, purchaseQuantity), GENERAL_WAIT_TIME).WaitAndGetOperationResult();
        }

        public IInventoryServiceCompletedMessage Hold(IActorRef inventoryActor, int holdQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(new PlaceHoldMessage(productId, holdQuantity), GENERAL_WAIT_TIME).WaitAndGetOperationResult();
        }

        public IInventoryServiceCompletedMessage UpdateQuantityAndHold(IActorRef inventoryActor, int holdQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(new UpdateAndHoldQuantityMessage(productId, holdQuantity), GENERAL_WAIT_TIME).WaitAndGetOperationResult();
        }

        public IInventoryServiceCompletedMessage PurchaseFromHolds(IActorRef inventoryActor, int purchaseQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(new PurchaseFromHoldsMessage(productId, purchaseQuantity), GENERAL_WAIT_TIME).WaitAndGetOperationResult();
        }

        public IInventoryServiceCompletedMessage GetInventory(IActorRef inventoryActor, string inventoryName)
        {
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(new GetInventoryMessage(inventoryName), GENERAL_WAIT_TIME).WaitAndGetOperationResult();
        }
    }
}