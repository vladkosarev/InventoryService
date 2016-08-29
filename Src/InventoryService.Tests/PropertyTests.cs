using Akka.Actor;
using Akka.TestKit.Xunit2;
using FsCheck;
using FsCheck.Xunit;
using InventoryService.Actors;
using InventoryService.Messages.Request;
using InventoryService.Messages.Response;
using InventoryService.Storage;
using InventoryService.Storage.InMemoryLib;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace InventoryService.Tests
{
    public class PropertyTests : TestKit
    {
        public class Inventory
        {
            public Inventory(string name, int quantity, int reservations, int holds)
            {
                Name = name;
                Quantity = quantity;
                Reservations = reservations;
                Holds = holds;
            }

            public string Name { get; set; }
            public int Quantity { get; set; }
            public int Reservations { get; set; }
            public int Holds { get; set; }
        }

        public static class InventoryArbitrary
        {
            public static Arbitrary<Inventory> Inventories()
            {
                var genInventories = from name in Arb.Generate<Guid>()
                                     from quantity in Arb.Generate<uint>()
                                     from reservations in Gen.Choose(0, (int)quantity)
                                     from holds in Gen.Choose(0, (int)quantity)
                                     select new Inventory(name.ToString(), (int)quantity, reservations, holds);
                return genInventories.ToArbitrary();
            }
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Reservation_Test(Inventory inventory, uint toReserve)
        {
            bool initializationSuccess;
            var inventoryActor = TryInitializeInventoryServiceRepository(inventory, out initializationSuccess);
            if (!initializationSuccess) return;
            var alreadyReserved = inventory.Reservations;
            var r = Reserve(inventoryActor, (int)toReserve, inventory.Name);

            if (inventory.Quantity - inventory.Holds - inventory.Reservations - toReserve >= 0)
            {
                alreadyReserved += (int)toReserve;
                Assert.True(r.Successful);
                Assert.Equal(r.ReservationQuantity, alreadyReserved);
            }
            else
            {
                Assert.False(r.Successful);

                Assert.Equal(r.ReservationQuantity, 0);
                //-todo when there is a failure, return nothing   Assert.Equal(r.ReservationQuantity, (int)toReserve);
            }
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Purchase_Test(Inventory inventory, uint toPurchase)
        {
            bool initializationSuccess;
            var inventoryActor = TryInitializeInventoryServiceRepository(inventory, out initializationSuccess);
            if (!initializationSuccess) return;
            var alreadyPurchased = inventory.Quantity;

            var r = Purchase(inventoryActor, (int)toPurchase, inventory.Name);

            if (inventory.Quantity - inventory.Holds - toPurchase >= 0)
            {
                alreadyPurchased -= (int)toPurchase;
                Assert.True(r.Successful);
                Assert.Equal(r.Quantity, alreadyPurchased);
            }
            else
            {
                Assert.False(r.Successful);
                Assert.Equal(r.Quantity, 0);
                //-todo when there is a failure, return nothing      Assert.Equal(r.Quantity, (int)toPurchase);
            }
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Holds_Test(Inventory inventory, uint toHold)
        {
            bool initializationSuccess;
            var inventoryActor = TryInitializeInventoryServiceRepository(inventory, out initializationSuccess);
            if (!initializationSuccess) return;
            var alreadyHeld = inventory.Holds;

            var r = Hold(inventoryActor, (int)toHold, inventory.Name);

            if (inventory.Quantity - inventory.Holds - toHold-inventory.Reservations >= 0)
            {
                alreadyHeld += (int)toHold;
                Assert.True(r.Successful);
                Assert.Equal(r.Holds, alreadyHeld);
            }
            else
            {
                Assert.False(r.Successful);
                Assert.Equal(r.Holds, 0);
                //-todo when there is a failure, return nothing   Assert.Equal(r.Holds, (int)toHold);
            }
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void UpdateQuantity_Test(Inventory inventory, int toUpdate)
        {
            bool initializationSuccess;
            var inventoryActor = TryInitializeInventoryServiceRepository(inventory, out initializationSuccess);
            if (!initializationSuccess) return;
            var r = UpdateQuantity(inventoryActor, toUpdate, inventory.Name);

            Assert.True(r.Successful);
            //if (item.Quantity - item.Holds - toHold >= 0)
            //{
            //    Assert.True(r.Successful);
            //    Assert.Equal(r.Holds, (int)toHold);
            //}
            //else
            //{
            //    Assert.False(r.Successful);
            //    Assert.Equal(r.Holds, (int)toHold);
            //}
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void PurchaseFromHolds_Test(Inventory inventory, uint toPurchase)
        {
            bool initializationSuccess;
            var inventoryActor = TryInitializeInventoryServiceRepository(inventory, out initializationSuccess);
            if (!initializationSuccess) return;
            var alreadyPurchased = inventory.Quantity;

            var r = PurchaseFromHolds(inventoryActor, (int)toPurchase, inventory.Name);

            if (inventory.Holds >= toPurchase && inventory.Quantity - toPurchase >= 0)
            {
                alreadyPurchased -= (int)toPurchase;
                Assert.True(r.Successful);
                Assert.Equal(r.Quantity, alreadyPurchased);
            }
            else
            {
                Assert.False(r.Successful);
                Assert.Equal(r.Quantity, 0);
                //-todo when there is a failure, return nothing     Assert.Equal(r.Quantity, (int)toPurchase);
            }
        }

        public IActorRef TryInitializeInventoryServiceRepository(Inventory product, out bool successful)
        {
            var inventoryService = new InMemory();

            //improve this with parallel

            var result =
                inventoryService.WriteInventory(new RealTimeInventory(product.Name, product.Quantity,
                    product.Reservations, product.Holds));
            Task.WaitAll(result);

            successful = result.Result.IsSuccessful;

            var inventoryActor = Sys.ActorOf(Props.Create(() => new InventoryActor(inventoryService, new TestPerformanceService(), true)));
            return inventoryActor;
        }

        public UpdateQuantityCompletedMessage UpdateQuantity(IActorRef inventoryActor, int quantity, string productId = "product1")
        {
            return inventoryActor.Ask<UpdateQuantityCompletedMessage>(new UpdateQuantityMessage(productId, quantity), TimeSpan.FromSeconds(10000)).Result;
        }

        public ReserveCompletedMessage Reserve(IActorRef inventoryActor, int reserveQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<ReserveCompletedMessage>(new ReserveMessage(productId, reserveQuantity), TimeSpan.FromSeconds(10000)).Result;
        }

        public PurchaseCompletedMessage Purchase(IActorRef inventoryActor, int purchaseQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<PurchaseCompletedMessage>(new PurchaseMessage(productId, purchaseQuantity), TimeSpan.FromSeconds(10000)).Result;
        }

        public PlaceHoldCompletedMessage Hold(IActorRef inventoryActor, int holdQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<PlaceHoldCompletedMessage>(new PlaceHoldMessage(productId, holdQuantity), TimeSpan.FromSeconds(10000)).Result;
        }

        public PurchaseFromHoldsCompletedMessage PurchaseFromHolds(IActorRef inventoryActor, int purchaseQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<PurchaseFromHoldsCompletedMessage>(new PurchaseFromHoldsMessage(productId, purchaseQuantity), TimeSpan.FromSeconds(10000)).Result;
        }
    }
}