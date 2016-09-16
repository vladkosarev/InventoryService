using Akka.Util.Internal;
using FsCheck.Xunit;
using InventoryService.AkkaInMemoryServer;
using InventoryService.Messages;
using InventoryService.Messages.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Random = System.Random;

namespace InventoryService.Tests
{
    public class PropertyTests
    {
        //public PropertyTests()
        //{
        //    //InventoryStorageName = "Esent_DB-" + Guid.NewGuid();
        //    //_inventoryStorage = new Esent(InventoryStorageName);
        //    InventoryStorage = new Storage.InMemoryLib.InMemory();
        //}

        ////~PropertyTests()
        ////{
        ////    _inventoryStorage.FlushAsync();
        ////    _inventoryStorage.Dispose();
        ////    if (Directory.Exists(InventoryStorageName))
        ////    {
        ////        new DirectoryInfo(InventoryStorageName).Delete(true);
        ////    }
        ////}

        ////private string InventoryStorageName { get; set; }
        //private IInventoryStorage InventoryStorage { get; set; }

        [Fact]
        public void Basic_Test()
        {
            var produltId = "sample" + Guid.NewGuid().ToString().Replace("-", "");
            foreach (var request in new List<Tuple<RealTimeInventory, int>>()
            {
                new Tuple<RealTimeInventory, int>(new RealTimeInventory(produltId, 10,0,0),2)
                    //  new Tuple<RealTimeInventory, int>(new RealTimeInventory(produltId, -8,8,11),-42),
                      //new Tuple<RealTimeInventory, int>(new RealTimeInventory(produltId, -3,2,2),-17)
            })
            {
                Enumerable.Range(1, 6).ForEach(oo =>
                  {
                      using (var testHelper = new InventoryServiceServer(request.Item1))
                      {
                        //var  response = testHelper.ReserveAsync(request.Item1, request.Item2).WaitAndGetOperationResult();
                        //  response = testHelper.ReserveAsync(request.Item1, request.Item2).WaitAndGetOperationResult();
                        var operation = InventoryServiceSpecificationHelper.GetOperations(testHelper)[oo];
                          InventoryServiceSpecificationHelper.GetAssertions()[oo](request.Item1, request.Item2, operation(request.Item1, request.Item2).WaitAndGetOperationResult());
                      }
                  });
            }
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Reservation_Test(RealTimeInventory inventory, int toReserve)
        {
            IInventoryServiceCompletedMessage response;
            using (var testHelper = new InventoryServiceServer(inventory))
                response = testHelper.ReserveAsync(inventory, toReserve).WaitAndGetOperationResult();

            InventoryServiceSpecificationHelper.AssertReservations(inventory, toReserve, response);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Purchase_Test(RealTimeInventory inventory, uint toPurchase)
        {
            IInventoryServiceCompletedMessage response;
            using (var testHelper = new InventoryServiceServer(inventory))
                response = testHelper.PurchaseAsync(inventory, (int)toPurchase).WaitAndGetOperationResult();

            InventoryServiceSpecificationHelper.AssertPurchase(inventory, toPurchase, response);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Holds_Test(RealTimeInventory inventory, int toHold)
        {
            IInventoryServiceCompletedMessage response;
            using (var testHelper = new InventoryServiceServer(inventory))
                response = testHelper.PlaceHoldAsync(inventory, toHold).WaitAndGetOperationResult();

            InventoryServiceSpecificationHelper.AssertHolds(inventory, toHold, response);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void UpadeteQuantityAndHold_Test(RealTimeInventory inventory, uint toHold)
        {
            IInventoryServiceCompletedMessage response;
            using (var testHelper = new InventoryServiceServer(inventory))
                response = testHelper.UpdateQuantityAndHoldAsync(inventory, (int)toHold).WaitAndGetOperationResult();

            InventoryServiceSpecificationHelper.AssertUpdateQuantityAndHold(inventory, toHold, response);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void UpdateQuantity_Test(RealTimeInventory inventory, int toUpdate)
        {
            IInventoryServiceCompletedMessage response;
            using (var testHelper = new InventoryServiceServer(inventory))
                response = testHelper.UpdateQuantityAsync(inventory, toUpdate).WaitAndGetOperationResult();

            InventoryServiceSpecificationHelper.AssertUpdateQuantity(inventory, toUpdate, response);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void PurchaseFromHolds_Test(RealTimeInventory inventory, uint toPurchase)
        {
            IInventoryServiceCompletedMessage response;
            using (var testHelper = new InventoryServiceServer(inventory))
                response = testHelper.PurchaseFromHoldsAsync(inventory, (int)toPurchase).WaitAndGetOperationResult();

            InventoryServiceSpecificationHelper.AssertPurchaseFromHolds(inventory, toPurchase, response);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Holds_Reservation_PurchaseFromHold_And_Purchase_Test(RealTimeInventory inventory, int toUpdate)
        {
            using (var testHelper = new InventoryServiceServer(inventory))
            {
                const int looplength = 5;
                var operations = InventoryServiceSpecificationHelper.GetOperations(testHelper);
                var assertions = InventoryServiceSpecificationHelper.GetAssertions();
                var updatedInventory = inventory;
                for (var i = 0; i < looplength; i++)
                {
                    var selection = new Random().Next(1, operations.Count);
                    var updatedInventoryMessage = operations[selection](updatedInventory, toUpdate).WaitAndGetOperationResult();
                    assertions[selection](updatedInventory, toUpdate, updatedInventoryMessage);
                    updatedInventory = updatedInventoryMessage.RealTimeInventory as RealTimeInventory;
                }
            }
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Concurrent_Holds_Reservation_PurchaseFromHold_And_Purchase_Sync_Test(RealTimeInventory inventory, int toUpdate)
        {
            const int looplength = 5;

            var events = CreateInventoryOperationEvents(inventory, toUpdate, looplength);

            RealTimeInventory currentInventoryAfterFirstOperation;
            using (var testHelper = new InventoryServiceServer(inventory))
                currentInventoryAfterFirstOperation = RunSomeInventoryOperationUsingEventsSync(events, testHelper);

            RealTimeInventory currentInventoryAfterLastOperation;
            using (var testHelper = new InventoryServiceServer(inventory))
                currentInventoryAfterLastOperation = RunSomeInventoryOperationUsingEventsSync(events, testHelper);

            Assert.Equal(currentInventoryAfterFirstOperation.Quantity, currentInventoryAfterLastOperation.Quantity);
            Assert.Equal(currentInventoryAfterFirstOperation.Reserved, currentInventoryAfterLastOperation.Reserved);
            Assert.Equal(currentInventoryAfterFirstOperation.Holds, currentInventoryAfterLastOperation.Holds);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Concurrent_Holds_Reservation_PurchaseFromHold_And_Purchase_Async_Test(RealTimeInventory inventory, int toUpdate)
        {
            const int looplength = 5;

            var events = CreateInventoryOperationEvents(inventory, toUpdate, looplength);

            RealTimeInventory currentInventoryAfterFirstOperation;
            using (var testHelper = new InventoryServiceServer(inventory))
                currentInventoryAfterFirstOperation = RunSomeInventoryOperationUsingEventsSync(events, testHelper);

            RealTimeInventory currentInventoryAfterLastOperation;
            using (var testHelper = new InventoryServiceServer(inventory))
                currentInventoryAfterLastOperation = RunSomeInventoryOperationUsingEventsAsync(events, testHelper);

            Assert.Equal(currentInventoryAfterFirstOperation.Quantity, currentInventoryAfterLastOperation.Quantity);
            Assert.Equal(currentInventoryAfterFirstOperation.Reserved, currentInventoryAfterLastOperation.Reserved);
            Assert.Equal(currentInventoryAfterFirstOperation.Holds, currentInventoryAfterLastOperation.Holds);
        }

        private static List<Tuple<int, RealTimeInventory, int>> CreateInventoryOperationEvents(RealTimeInventory inventory, int toUpdate, int looplength)
        {
            List<Tuple<int, RealTimeInventory, int>> events = new List<Tuple<int, RealTimeInventory, int>>();
            var operations = InventoryServiceSpecificationHelper.GetOperations(null);
            var updatedInventory = inventory;
            for (var i = 0; i < looplength; i++)
            {
                var selection = new Random().Next(1, operations.Count);
                events.Add(new Tuple<int, RealTimeInventory, int>(selection, new RealTimeInventory(updatedInventory.ProductId, updatedInventory.Quantity, updatedInventory.Reserved, updatedInventory.Holds), toUpdate));
            }
            return events;
        }

        private static RealTimeInventory RunSomeInventoryOperationUsingEventsSync(List<Tuple<int, RealTimeInventory, int>> events, InventoryServiceServer testHelper)
        {
            var operations = InventoryServiceSpecificationHelper.GetOperations(testHelper);

            IInventoryServiceCompletedMessage currentInventoryAfterLastOperationResult = null;
            foreach (var @event in events)
            {
                currentInventoryAfterLastOperationResult = operations[@event.Item1](@event.Item2, @event.Item3).WaitAndGetOperationResult();
            }
            var currentInventoryAfterLastOperation = currentInventoryAfterLastOperationResult.RealTimeInventory;
            return currentInventoryAfterLastOperation as RealTimeInventory;
        }

        private static RealTimeInventory RunSomeInventoryOperationUsingEventsAsync(List<Tuple<int, RealTimeInventory, int>> events, InventoryServiceServer testHelper)
        {
            var operations2 = InventoryServiceSpecificationHelper.GetOperations(testHelper);

            Task<IInventoryServiceCompletedMessage> currentInventoryAfterLastOperationResult = null;
            foreach (var @event in events)
            {
                currentInventoryAfterLastOperationResult = operations2[@event.Item1](@event.Item2, @event.Item3);
            }
            var currentInventoryAfterLastOperation = currentInventoryAfterLastOperationResult.WaitAndGetOperationResult().RealTimeInventory;
            return currentInventoryAfterLastOperation as RealTimeInventory;
        }
    }
}