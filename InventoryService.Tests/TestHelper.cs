using Akka.Actor;
using InventoryService.Actors;
using InventoryService.Messages.Request;
using InventoryService.Messages.Response;
using InventoryService.Storage;
using System;
using System.Threading.Tasks;
using InventoryService.Messages.Models;

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
        public IActorRef TryInitializeInventoryServiceRepository(PropertyTests.Inventory product, ActorSystem sys, out bool successful)
        {
            //  var inventoryService = new InMemoryDictionary();// new InMemory();
            try
            {
                //improve this with parallel
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

        public ICompletedMessage UpdateQuantity(IActorRef inventoryActor, int quantity, string productId = "product1")
        {
            return inventoryActor.Ask<ICompletedMessage>(new UpdateQuantityMessage(productId, quantity), GENERAL_WAIT_TIME).Result;
        }

        public ICompletedMessage Reserve(IActorRef inventoryActor, int reserveQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<ICompletedMessage>(new ReserveMessage(productId, reserveQuantity), GENERAL_WAIT_TIME).Result;
        }

        public ICompletedMessage Purchase(IActorRef inventoryActor, int purchaseQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<ICompletedMessage>(new PurchaseMessage(productId, purchaseQuantity), GENERAL_WAIT_TIME).Result;
        }

        public ICompletedMessage Hold(IActorRef inventoryActor, int holdQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<ICompletedMessage>(new PlaceHoldMessage(productId, holdQuantity), GENERAL_WAIT_TIME).Result;
        }

        public ICompletedMessage PurchaseFromHolds(IActorRef inventoryActor, int purchaseQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<ICompletedMessage>(new PurchaseFromHoldsMessage(productId, purchaseQuantity), GENERAL_WAIT_TIME).Result;
        }

        public ICompletedMessage GetInventory(IActorRef inventoryActor, string inventoryName)
        {
            return inventoryActor.Ask<ICompletedMessage>(new GetInventoryMessage(inventoryName, true), GENERAL_WAIT_TIME).Result;
        }
    }
}