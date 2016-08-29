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
using System.Collections.Generic;
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
            var l = new List<Inventory> { inventory };
            var inventoryActor = InitializeInventoryServiceRepository(l);

            foreach (var item in l)
            {
                var r = Reserve(inventoryActor, (int)toReserve, item.Name);

                if (item.Quantity - item.Holds - item.Reservations - toReserve >= 0)
                {
                    Assert.True(r.Successful);
                    Assert.Equal(r.ReservationQuantity, (int)toReserve);
                }
                else
                {
                    Assert.False(r.Successful);
                    Assert.Equal(r.ReservationQuantity, (int)toReserve);
                }
            }
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Purchase_Test(Inventory inventory, uint toPurchase)
        {
            var l = new List<Inventory> { inventory };
            var inventoryActor = InitializeInventoryServiceRepository(l);

            foreach (var item in l)
            {
                var r = Purchase(inventoryActor, (int)toPurchase, item.Name);

                if (item.Quantity - item.Holds - toPurchase >= 0)
                {
                    Assert.True(r.Successful);
                    Assert.Equal(r.Quantity, (int)toPurchase);
                }
                else
                {
                    Assert.False(r.Successful);
                    Assert.Equal(r.Quantity, (int)toPurchase);
                }
            }
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Holds_Test(Inventory inventory, uint toHold)
        {
            var l = new List<Inventory> { inventory };
            var inventoryActor = InitializeInventoryServiceRepository(l);

            foreach (var item in l)
            {
                var r = Hold(inventoryActor, (int)toHold, item.Name);

                if (item.Quantity - item.Holds - toHold >= 0)
                {
                    Assert.True(r.Successful);
                    Assert.Equal(r.Holds, (int)toHold);
                }
                else
                {
                    Assert.False(r.Successful);
                    Assert.Equal(r.Holds, (int)toHold);
                }
            }
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void UpdateQuantity_Test(Inventory inventory, int toUpdate)
        {
            var l = new List<Inventory> { inventory };
            var inventoryActor = InitializeInventoryServiceRepository(l);

            foreach (var item in l)
            {
                var r = UpdateQuantity(inventoryActor, toUpdate, item.Name);

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
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void PurchaseFromHolds_Test(Inventory inventory, uint toPurchase)
        {
            var l = new List<Inventory> { inventory };
            var inventoryActor = InitializeInventoryServiceRepository(l);

            foreach (var item in l)
            {
                var r = PurchaseFromHolds(inventoryActor, (int)toPurchase, item.Name);

                if (item.Holds >= toPurchase && item.Quantity - toPurchase >= 0)
                {
                    Assert.True(r.Successful);
                    Assert.Equal(r.Quantity, (int)toPurchase);
                }
                else
                {
                    Assert.False(r.Successful);
                    Assert.Equal(r.Quantity, (int)toPurchase);
                }
            }
        }

        public IActorRef InitializeInventoryServiceRepository(IList<Inventory> productInventory)
        {
            var inventoryService = new InMemory();

            //improve this with parallel
            foreach (var product in productInventory)
            {
                Task.Run(() => inventoryService.WriteInventory(new RealTimeInventory(product.Name, product.Quantity, product.Reservations, product.Holds))).Wait();
            }

            var inventoryActor = Sys.ActorOf(Props.Create(() => new InventoryActor(inventoryService, new TestPerformanceService(), true)));
            return inventoryActor;
        }

        public UpdateQuantityCompletedMessage UpdateQuantity(IActorRef inventoryActor, int quantity, string productId = "product1")
        {
            return inventoryActor.Ask<UpdateQuantityCompletedMessage>(new UpdateQuantityMessage(productId, quantity), TimeSpan.FromSeconds(1)).Result;
        }

        public ReserveCompletedMessage Reserve(IActorRef inventoryActor, int reserveQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<ReserveCompletedMessage>(new ReserveMessage(productId, reserveQuantity), TimeSpan.FromSeconds(1)).Result;
        }

        public PurchaseCompletedMessage Purchase(IActorRef inventoryActor, int purchaseQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<PurchaseCompletedMessage>(new PurchaseMessage(productId, purchaseQuantity), TimeSpan.FromSeconds(1)).Result;
        }

        public PlaceHoldCompletedMessage Hold(IActorRef inventoryActor, int holdQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<PlaceHoldCompletedMessage>(new PlaceHoldMessage(productId, holdQuantity), TimeSpan.FromSeconds(1)).Result;
        }

        public PurchaseFromHoldsCompletedMessage PurchaseFromHolds(IActorRef inventoryActor, int purchaseQuantity, string productId = "product1")
        {
            return inventoryActor.Ask<PurchaseFromHoldsCompletedMessage>(new PurchaseFromHoldsMessage(productId, purchaseQuantity), TimeSpan.FromSeconds(1)).Result;
        }
    }
}