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

        public IActorRef TryInitializeInventoryServiceRepository(Inventory product, ActorSystem sys, out bool successful)
        {
            //  var inventoryService = new InMemoryDictionary();// new InMemory();
            try
            {
               
                var result = InventoryService.WriteInventory(new RealTimeInventory(product.Name, product.Quantity, product.Reserved, product.Holds));
                Task.WaitAll(result);
                successful = result.Result.IsSuccessful;
            }
            catch (Exception)
            {
                successful = false;
            }
            var inventoryActor = sys.ActorOf(Props.Create(() => new InventoryActor(InventoryService, new TestPerformanceService(), true)));
            return inventoryActor;
        }

        public IInventoryServiceCompletedMessage UpdateQuantity(IActorRef inventoryActor, int quantity, string productId = "product1")
        {
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(new UpdateQuantityMessage(productId, quantity), GENERAL_WAIT_TIME).Result;
        }

        public IInventoryServiceCompletedMessage Reserve(IActorRef inventoryActor, int reserveQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(new ReserveMessage(productId, reserveQuantity), GENERAL_WAIT_TIME).Result;
        }

        public IInventoryServiceCompletedMessage Purchase(IActorRef inventoryActor, int purchaseQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(new PurchaseMessage(productId, purchaseQuantity), GENERAL_WAIT_TIME).Result;
        }

        public IInventoryServiceCompletedMessage Hold(IActorRef inventoryActor, int holdQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(new PlaceHoldMessage(productId, holdQuantity), GENERAL_WAIT_TIME).Result;
        }

        public IInventoryServiceCompletedMessage UpdateQuantityAndHold(IActorRef inventoryActor, int holdQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(new UpdateAndHoldQuantityMessage(productId, holdQuantity), GENERAL_WAIT_TIME).Result;
        }

        public IInventoryServiceCompletedMessage PurchaseFromHolds(IActorRef inventoryActor, int purchaseQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(new PurchaseFromHoldsMessage(productId, purchaseQuantity), GENERAL_WAIT_TIME).Result;
        }

        public IInventoryServiceCompletedMessage GetInventory(IActorRef inventoryActor, string inventoryName)
        {
            return inventoryActor.Ask<IInventoryServiceCompletedMessage>(new GetInventoryMessage(inventoryName, true), GENERAL_WAIT_TIME).Result;
        }
    }
}