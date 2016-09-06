using Akka.Actor;
using Akka.TestKit.Xunit2;
using FsCheck.Xunit;
using InventoryService.Messages.Models;
using InventoryService.Messages.Response;
using InventoryService.Storage;
using InventoryService.Storage.EsentLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Random = System.Random;

namespace InventoryService.Tests
{
    public class PropertyTests : TestKit
    {
        public PropertyTests()
        {
            InventoryStorageName = "Esent_DB-"+Guid.NewGuid();
            _inventoryStorage = new Esent(InventoryStorageName);
        }

        ~PropertyTests()
        {
            _inventoryStorage.FlushAsync();
            _inventoryStorage.Dispose();
            if (Directory.Exists(InventoryStorageName))
            {
                new DirectoryInfo(InventoryStorageName).Delete(true);
            }
        }

        private string InventoryStorageName { get; set; }
        private IInventoryStorage _inventoryStorage { get; set; }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Reservation_Test(RealTimeInventory inventory, int toReserve)
        {
            var testHelper = new TestHelper(_inventoryStorage);
            var inventoryActor = testHelper.InitializeAndGetInventoryActor(inventory, Sys);

            var response = testHelper.Reserve(inventoryActor, (int)toReserve, inventory.ProductId).WaitAndGetOperationResult();

            InventoryServiceSpecificationHelper.AssertReservations(inventory, toReserve, response);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Purchase_Test(RealTimeInventory inventory, uint toPurchase)
        {
            var testHelper = new TestHelper(_inventoryStorage);
            var inventoryActor = testHelper.InitializeAndGetInventoryActor(inventory, Sys);

            var response = testHelper.Purchase(inventoryActor, (int)toPurchase, inventory.ProductId).WaitAndGetOperationResult();

            InventoryServiceSpecificationHelper.AssertPurchase(inventory, toPurchase, response);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Holds_Test(RealTimeInventory inventory, int toHold)
        {
            var testHelper = new TestHelper(_inventoryStorage);
            var inventoryActor = testHelper.InitializeAndGetInventoryActor(inventory, Sys);

            var response = testHelper.Hold(inventoryActor, toHold, inventory.ProductId).WaitAndGetOperationResult();

            InventoryServiceSpecificationHelper.AssertHolds(inventory, toHold, response);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void UpadeteQuantityAndHold_Test(RealTimeInventory inventory, uint toHold)
        {
            var testHelper = new TestHelper(_inventoryStorage);
            var inventoryActor = testHelper.InitializeAndGetInventoryActor(inventory, Sys);

            var response = testHelper.UpdateQuantityAndHold(inventoryActor, (int)toHold, inventory.ProductId).WaitAndGetOperationResult();

            InventoryServiceSpecificationHelper.AssertUpdateQuantityAndHold(inventory, toHold, response);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void UpdateQuantity_Test(RealTimeInventory inventory, int toUpdate)
        {
            var testHelper = new TestHelper(_inventoryStorage);
            var inventoryActor = testHelper.InitializeAndGetInventoryActor(inventory, Sys);

            var response = testHelper.UpdateQuantity(inventoryActor, toUpdate, inventory.ProductId).WaitAndGetOperationResult();

            InventoryServiceSpecificationHelper.AssertUpdateQuantity(inventory, toUpdate, response);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void PurchaseFromHolds_Test(RealTimeInventory inventory, uint toPurchase)
        {
            var testHelper = new TestHelper(_inventoryStorage);
            var inventoryActor = testHelper.InitializeAndGetInventoryActor(inventory, Sys);

            var response = testHelper.PurchaseFromHolds(inventoryActor, (int)toPurchase, inventory.ProductId).WaitAndGetOperationResult();

            InventoryServiceSpecificationHelper.AssertPurchaseFromHolds(inventory, toPurchase, response);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Holds_Reservation_PurchaseFromHold_And_Purchase_Test(RealTimeInventory inventory, int toUpdate)
        {
            var testHelper = new TestHelper(_inventoryStorage);
            var inventoryActor = testHelper.InitializeAndGetInventoryActor(inventory, Sys);
            const int looplength = 5;
            var operations = InventoryServiceSpecificationHelper.GetOperations(testHelper, inventoryActor);
            var assertions = InventoryServiceSpecificationHelper.GetAssertions(testHelper, inventoryActor);
            var updatedInventory = inventory;
            for (var i = 0; i < looplength; i++)
            {
                var selection = new Random().Next(1, operations.Count);
                var updatedInventoryMessage = operations[selection](updatedInventory.ProductId, toUpdate).WaitAndGetOperationResult();
                assertions[selection](updatedInventory, toUpdate, updatedInventoryMessage);
                updatedInventory = updatedInventoryMessage.RealTimeInventory as RealTimeInventory;
            }
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Concurrent_Holds_Reservation_PurchaseFromHold_And_Purchase_Sync_Test(RealTimeInventory inventory, int toUpdate)
        {
            var testHelper = new TestHelper(_inventoryStorage);
            const int looplength = 5;

            var events = CreateInventoryOperationEvents(inventory, toUpdate, testHelper, CreateANewInventoryService(inventory, testHelper), looplength);

            var inventoryActor1 = CreateANewInventoryService(inventory, testHelper);
            var currentInventoryAfterFirstOperation = RunSomeInventoryOperationUsingEventsSync(events, testHelper, inventoryActor1);

            var inventoryActor2 = CreateANewInventoryService(inventory, testHelper);
            var currentInventoryAfterLastOperation = RunSomeInventoryOperationUsingEventsSync(events, testHelper, inventoryActor2);

            Assert.Equal(currentInventoryAfterFirstOperation.Quantity, currentInventoryAfterLastOperation.Quantity);
            Assert.Equal(currentInventoryAfterFirstOperation.Reserved, currentInventoryAfterLastOperation.Reserved);
            Assert.Equal(currentInventoryAfterFirstOperation.Holds, currentInventoryAfterLastOperation.Holds);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Concurrent_Holds_Reservation_PurchaseFromHold_And_Purchase_Async_Test(RealTimeInventory inventory, int toUpdate)
        {
            var testHelper = new TestHelper(_inventoryStorage);
            const int looplength = 5;

            var events = CreateInventoryOperationEvents(inventory, toUpdate, testHelper, CreateANewInventoryService(inventory, testHelper), looplength);

            var inventoryActor1 = CreateANewInventoryService(inventory, testHelper);
            var currentInventoryAfterFirstOperation = RunSomeInventoryOperationUsingEventsSync(events, testHelper, inventoryActor1);

            var inventoryActor2 = CreateANewInventoryService(inventory, testHelper);
            var currentInventoryAfterLastOperation = RunSomeInventoryOperationUsingEventsAsync(events, testHelper, inventoryActor2);

            Assert.Equal(currentInventoryAfterFirstOperation.Quantity, currentInventoryAfterLastOperation.Quantity);
            Assert.Equal(currentInventoryAfterFirstOperation.Reserved, currentInventoryAfterLastOperation.Reserved);
            Assert.Equal(currentInventoryAfterFirstOperation.Holds, currentInventoryAfterLastOperation.Holds);
        }

        private static List<Tuple<int, RealTimeInventory, int>> CreateInventoryOperationEvents(RealTimeInventory inventory, int toUpdate, TestHelper testHelper, IActorRef inventoryActor1, int looplength)
        {
            List<Tuple<int, RealTimeInventory, int>> events = new List<Tuple<int, RealTimeInventory, int>>();
            var operations = InventoryServiceSpecificationHelper.GetOperations(testHelper, inventoryActor1);
            var updatedInventory = inventory;
            for (var i = 0; i < looplength; i++)
            {
                var selection = new Random().Next(1, operations.Count);
                events.Add(new Tuple<int, RealTimeInventory, int>(selection, new RealTimeInventory(updatedInventory.ProductId, updatedInventory.Quantity, updatedInventory.Reserved, updatedInventory.Holds), toUpdate));
            }
            return events;
        }

        private static RealTimeInventory RunSomeInventoryOperationUsingEventsSync(List<Tuple<int, RealTimeInventory, int>> events, TestHelper testHelper, IActorRef inventoryActor2)
        {
            var operations = InventoryServiceSpecificationHelper.GetOperations(testHelper, inventoryActor2);

            IInventoryServiceCompletedMessage currentInventoryAfterLastOperationResult = null;
            foreach (var @event in events)
            {
                currentInventoryAfterLastOperationResult = operations[@event.Item1](@event.Item2.ProductId, @event.Item3).WaitAndGetOperationResult();
            }
            var currentInventoryAfterLastOperation = currentInventoryAfterLastOperationResult.RealTimeInventory;
            return currentInventoryAfterLastOperation as RealTimeInventory;
        }

        private static RealTimeInventory RunSomeInventoryOperationUsingEventsAsync(List<Tuple<int, RealTimeInventory, int>> events, TestHelper testHelper, IActorRef inventoryActor2)
        {
            var operations2 = InventoryServiceSpecificationHelper.GetOperations(testHelper, inventoryActor2);

            Task<IInventoryServiceCompletedMessage> currentInventoryAfterLastOperationResult = null;
            foreach (var @event in events)
            {
                currentInventoryAfterLastOperationResult = operations2[@event.Item1](@event.Item2.ProductId, @event.Item3);
            }
            var currentInventoryAfterLastOperation = currentInventoryAfterLastOperationResult.WaitAndGetOperationResult().RealTimeInventory;
            return currentInventoryAfterLastOperation as RealTimeInventory;
        }

        private IActorRef CreateANewInventoryService(RealTimeInventory inventory, TestHelper testHelper)
        {
            var inventoryActor2 = testHelper.InitializeAndGetInventoryActor(inventory, Sys);
            var currentInventory = testHelper.GetInventory(inventoryActor2, inventory.ProductId).WaitAndGetOperationResult().RealTimeInventory;
            Assert.Equal(currentInventory.Quantity, inventory.Quantity);
            Assert.Equal(currentInventory.Reserved, inventory.Reserved);
            Assert.Equal(currentInventory.Holds, inventory.Holds);
            return inventoryActor2;
        }
    }
}