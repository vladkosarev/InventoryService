using Akka.Actor;
using InventoryService.Actors;
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

        public UpdateQuantityCompletedMessage UpdateQuantity(IActorRef inventoryActor, int quantity, string productId = "product1")
        {
            return inventoryActor.Ask<UpdateQuantityCompletedMessage>(new UpdateQuantityMessage(productId, quantity), TimeSpan.FromSeconds(3)).Result;
        }

        public ReserveCompletedMessage Reserve(IActorRef inventoryActor, int reserveQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<ReserveCompletedMessage>(new ReserveMessage(productId, reserveQuantity), TimeSpan.FromSeconds(3)).Result;
        }

        public PurchaseCompletedMessage Purchase(IActorRef inventoryActor, int purchaseQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<PurchaseCompletedMessage>(new PurchaseMessage(productId, purchaseQuantity), TimeSpan.FromSeconds(3)).Result;
        }

        public PlaceHoldCompletedMessage Hold(IActorRef inventoryActor, int holdQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<PlaceHoldCompletedMessage>(new PlaceHoldMessage(productId, holdQuantity), TimeSpan.FromSeconds(3)).Result;
        }

        public PurchaseFromHoldsCompletedMessage PurchaseFromHolds(IActorRef inventoryActor, int purchaseQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<PurchaseFromHoldsCompletedMessage>(new PurchaseFromHoldsMessage(productId, purchaseQuantity), TimeSpan.FromSeconds(3)).Result;
        }

        public GetInventoryCompletedMessage GetInventory(IActorRef inventoryActor, string inventoryName)
        {
            return inventoryActor.Ask<GetInventoryCompletedMessage>(new GetInventoryMessage(inventoryName, true), TimeSpan.FromSeconds(3)).Result;
        }
    }
}