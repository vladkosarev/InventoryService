using Akka.TestKit.Xunit2;
using FsCheck.Xunit;
using InventoryService.Messages.Models;
using InventoryService.Storage.InMemoryLib;
using Random = System.Random;

namespace InventoryService.Tests
{
    public class PropertyTests : TestKit
    {
        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Reservation_Test(RealTimeInventory inventory, int toReserve)
        {
            var testHelper = new TestHelper(new InMemory());
            var inventoryActor = testHelper.InitializeAndGetInventoryActor(inventory, Sys);

            var r = testHelper.Reserve(inventoryActor, (int)toReserve, inventory.ProductId);

            InventoryServiceSpecificationHelper.AssertReservations(inventory, toReserve, r);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Purchase_Test(RealTimeInventory inventory, uint toPurchase)
        {
            var testHelper = new TestHelper(new InMemory());
            var inventoryActor = testHelper.InitializeAndGetInventoryActor(inventory, Sys);

            var r = testHelper.Purchase(inventoryActor, (int)toPurchase, inventory.ProductId);

            InventoryServiceSpecificationHelper.AssertPurchase(inventory, toPurchase, r);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Holds_Test(RealTimeInventory inventory, int toHold)
        {
            var testHelper = new TestHelper(new InMemory());
            var inventoryActor = testHelper.InitializeAndGetInventoryActor(inventory, Sys);

            var r = testHelper.Hold(inventoryActor, toHold, inventory.ProductId);

            InventoryServiceSpecificationHelper.AssertHolds(inventory, toHold, r);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void UpadeteQuantityAndHold_Test(RealTimeInventory inventory, uint toHold)
        {
            var testHelper = new TestHelper(new InMemory());
            var inventoryActor = testHelper.InitializeAndGetInventoryActor(inventory, Sys);

            var r = testHelper.UpdateQuantityAndHold(inventoryActor, (int)toHold, inventory.ProductId);

            InventoryServiceSpecificationHelper.AssertUpdateQuantityAndHold(inventory, toHold, r);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void UpdateQuantity_Test(RealTimeInventory inventory, int toUpdate)
        {
            var testHelper = new TestHelper(new InMemory());
            var inventoryActor = testHelper.InitializeAndGetInventoryActor(inventory, Sys);

            var r = testHelper.UpdateQuantity(inventoryActor, toUpdate, inventory.ProductId);

            InventoryServiceSpecificationHelper.AssertUpdateQuantity(inventory, toUpdate, r);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void PurchaseFromHolds_Test(RealTimeInventory inventory, uint toPurchase)
        {
            var testHelper = new TestHelper(new InMemory());
            var inventoryActor = testHelper.InitializeAndGetInventoryActor(inventory, Sys);

            var r = testHelper.PurchaseFromHolds(inventoryActor, (int)toPurchase, inventory.ProductId);

            InventoryServiceSpecificationHelper.AssertPurchaseFromHolds(inventory, toPurchase, r);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Concurrent_Holds_Reservation_PurchaseFromHold_And_Purchase_Test(RealTimeInventory inventory, int toUpdate)
        {
            var testHelper = new TestHelper(new InMemoryDictionary());
            var inventoryActor = testHelper.InitializeAndGetInventoryActor(inventory, Sys);
            const int looplength = 5;
            var operations = InventoryServiceSpecificationHelper.GetOperations(testHelper, inventoryActor);

            var updatedInventory = inventory;
            for (var i = 0; i < looplength; i++)
            {
                updatedInventory = operations[new Random().Next(1, operations.Count)](updatedInventory, toUpdate) as RealTimeInventory;
            }
        }
    }
}