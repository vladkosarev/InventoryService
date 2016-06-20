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
        public void Should_be_able_to_reserve_max_for_multiple_products(IList<Tuple<Guid, int, int>> l)
        {
            var initialInventory =
                l.Select(i => new Tuple<string, int, int>(i.Item1.ToString(), i.Item2, i.Item3)).ToList();

            var inventoryActor = InitializeInventoryServiceRepository(initialInventory);

            foreach (var item in l)
            {
                Assert.True(Reserve(inventoryActor, item.Item2 - item.Item3, item.Item1.ToString()));
            }
        }

        [Property]
        public void Should_not_be_able_to_reserve_over_max_for_multiple_products(IList<Tuple<Guid, int, int, uint>> l)
        {
            var initialInventory =
                l.Select(i => new Tuple<string, int, int>(i.Item1.ToString(), i.Item2, i.Item3)).ToList();

            var inventoryActor = InitializeInventoryServiceRepository(initialInventory);

            foreach (var item in l)
            {
                Assert.False(Reserve(inventoryActor, (int)(item.Item2 - item.Item3 + item.Item4 + 1), item.Item1.ToString()));
            }
        }

        [Property]
        public void Should_be_able_to_purchase_max(int i)
        {
            Purchase(i, 0, i);
        }

        [Property()]
        public void Should_not_be_able_to_reserve_more_than_max()
        {
            Prop.ForAll(
                Arb.From<int>()
                , Arb.From(Gen.Choose(1, int.MaxValue))
                , (total, overflow) =>
                {
                    var inventoryActor = InitializeInventoryServiceRepository(
                        new List<Tuple<string, int, int>>() { new Tuple<string, int, int>("product1", total, 0) });
                    // [("product1", total, 0)]
                    return !Reserve(inventoryActor, total + overflow);
                })
                .QuickCheckThrowOnFailure();
        }

        [Property]
        public void Should_not_be_able_to_purchase_more_than_max()
        {
            Prop.ForAll(
                Arb.From<int>()
                , Arb.From(Gen.Choose(1, int.MaxValue))
                , (total, overflow) => !Purchase(total, 0, total + overflow))
                .QuickCheckThrowOnFailure();
        }

        public IActorRef InitializeInventoryServiceRepository(IList<Tuple<string, int, int>> productInventory)
        {
            var inventoryService = new InMemory();

            //improve this with parallel
            foreach (var product in productInventory)
            {
                Task.Run(() => inventoryService.WriteInventory(product.Item1, product.Item2, product.Item3)).Wait();
            }

            var inventoryActor = Sys.ActorOf(Props.Create(() => new InventoryActor(inventoryService, new TestPerformanceService(), true)));
            return inventoryActor;
        }

        public bool Reserve(IActorRef inventoryActor, int reserveQuantity, string productId = "product1")
        {
            var result = inventoryActor.Ask<ReservedMessage>(new ReserveMessage(productId, reserveQuantity), TimeSpan.FromSeconds(1)).Result;

            return result.Successful;
        }

        public bool Purchase(int initialQuantity, int initialReservations, int purchaseQuantity, string productId = "product1")
        {
            var inventoryService = new InMemory();
            inventoryService.WriteInventory(productId, initialQuantity, initialReservations);

            var inventoryActor = Sys.ActorOf(Props.Create(() => new InventoryActor(inventoryService, new TestPerformanceService(), true)));

            var result = inventoryActor.Ask<PurchasedMessage>(new PurchaseMessage(productId, purchaseQuantity), TimeSpan.FromSeconds(1)).Result;

            return result.Successful;
        }
    }
}

