using System;
using Xunit;
using FsCheck;
using FsCheck.Xunit;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.TestKit.Xunit2;
using InventoryService.Actors;
using InventoryService.Messages;
using InventoryService.Storage;

namespace InventoryService.Tests
{
    public class PropertyTests : TestKit
    {
        [Property]
        public void Should_be_able_to_reserve_max_for_multiple_products(IList<Tuple<Guid, int, int, int>> l)
        {
            var initialInventory =
                l.Select(i => new Tuple<string, int, int, int>(i.Item1.ToString(), i.Item2, i.Item3, i.Item4)).ToList();

            var inventoryActor = InitializeInventoryServiceRepository(initialInventory);

            foreach (var item in l)
            {
                Assert.True(Reserve(inventoryActor, item.Item2 - item.Item3, item.Item1.ToString()));
            }
        }

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
                                     from quantity in Arb.Generate<int>()
                                     from reservations in Arb.Generate<int>()
                                     from holds in Arb.Generate<int>()
                                     select new Inventory(name.ToString(), quantity, reservations, holds);
                return genInventories.ToArbitrary();
            }
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Should_not_be_able_to_reserve_more_than_max2(Inventory inventory)
        {
            var l = new List<Inventory> {inventory};
            var inventoryActor = InitializeInventoryServiceRepository(l);

            foreach (var item in l)
            {
                Assert.False(Reserve(inventoryActor, item.Quantity - item.Reservations, item.Name));
            }
        }

        //[Property]
        //public Property Should_be_able_to_reserve_max_for_multiple_products2()
        //{
        //    var initialQuantities = new List<Inventory>();
        //    Prop.ForAll(InventoryArbitrary.Inventories(), i => {
        //        Console.WriteLine(i);
        //        initialQuantities.Add(i);
        //    });

        //var inventoryActor = InitializeInventoryServiceRepository(initialQuantities);

        //    foreach (var item in initialQuantities)
        //    {
        //        Assert.True(Reserve(inventoryActor, item.Reservations - item.Quantity, item.Name));
        //    }
        //}

        [Property]
        public void Should_not_be_able_to_reserve_over_max_for_multiple_products(IList<Tuple<Guid, int, int, int, uint>> l)
        {
            var initialInventory =
                l.Select(i => new Tuple<string, int, int, int>(i.Item1.ToString(), i.Item2, i.Item3, i.Item4)).ToList();

            var inventoryActor = InitializeInventoryServiceRepository(initialInventory);

            foreach (var item in l)
            {
                Assert.False(Reserve(inventoryActor, (int)(item.Item2 - item.Item3 + item.Item5 + 1), item.Item1.ToString()));
            }
        }

        [Property(Verbose = true)]
        public void test1()
        {
            var x = Gen.Choose(0, 10);
            
            Assert.True(true);
        }

        [Property]
        public void Should_not_be_able_to_reserve_more_than_max()
        {
            Prop.ForAll(
                Arb.From<int>()
                , Arb.From(Gen.Choose(1, int.MaxValue))
                , (total, overflow) =>
                {
                    var inventoryActor = InitializeInventoryServiceRepository(
                        new List<Tuple<string, int, int, int>>() { new Tuple<string, int, int, int>("product1", total, 0, 0) });
                    // [("product1", total, 0)]
                    return !Reserve(inventoryActor, total + overflow);
                })
                .QuickCheckThrowOnFailure();
        }

        [Property]
        public void Should_be_able_to_purchase_max(int i)
        {
            Purchase(i, 0, 0, i);
        }

        [Property]
        public void Should_not_be_able_to_purchase_more_than_max()
        {
            Prop.ForAll(
                Arb.From<int>()
                , Arb.From(Gen.Choose(1, int.MaxValue))
                , (total, overflow) => !Purchase(total, 0, 0, total + overflow))
                .QuickCheckThrowOnFailure();
        }

        public IActorRef InitializeInventoryServiceRepository(IList<Tuple<string, int, int, int>> productInventory)
        {
            var inventoryService = new InMemory();

            //improve this with parallel
            foreach (var product in productInventory)
            {
                Task.Run(() => inventoryService.WriteInventory(product.Item1, product.Item2, product.Item3, product.Item4)).Wait();
            }

            var inventoryActor = Sys.ActorOf(Props.Create(() => new InventoryActor(inventoryService, new TestPerformanceService(), true)));
            return inventoryActor;
        }

        public IActorRef InitializeInventoryServiceRepository(IList<Inventory> productInventory)
        {
            var inventoryService = new InMemory();

            //improve this with parallel
            foreach (var product in productInventory)
            {
                Task.Run(() => inventoryService.WriteInventory(product.Name, product.Quantity, product.Reservations, product.Holds)).Wait();
            }

            var inventoryActor = Sys.ActorOf(Props.Create(() => new InventoryActor(inventoryService, new TestPerformanceService(), true)));
            return inventoryActor;
        }

        public bool Reserve(IActorRef inventoryActor, int reserveQuantity, string productId = "product1")
        {
            var result = inventoryActor.Ask<ReservedMessage>(new ReserveMessage(productId, reserveQuantity), TimeSpan.FromSeconds(1)).Result;

            return result.Successful;
        }

        public bool Purchase(int initialQuantity, int initialReservations, int initialHolds, int purchaseQuantity, string productId = "product1")
        {
            var inventoryService = new InMemory();
            inventoryService.WriteInventory(productId, initialQuantity, initialReservations, initialHolds).Wait();

            var inventoryActor = Sys.ActorOf(Props.Create(() => new InventoryActor(inventoryService, new TestPerformanceService(), true)));

            var result = inventoryActor.Ask<PurchasedMessage>(new PurchaseMessage(productId, purchaseQuantity), TimeSpan.FromSeconds(1)).Result;

            return result.Successful;
        }
    }
}

