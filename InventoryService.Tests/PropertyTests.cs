using Akka.TestKit.Xunit2;
using FsCheck;
using FsCheck.Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using InventoryService.Messages.Response;
using Microsoft.FSharp.Collections;
using Xunit;

namespace InventoryService.Tests
{
    public class PropertyTests : TestKit
    {
        private readonly TestHelper _testHelper;

        public PropertyTests()
        {
            _testHelper = new TestHelper();
        }

        public class Inventory
        {
            public Inventory(string name, int quantity, int reservations, int holds)
            {
                Name = name;
                Quantity = quantity;
                Reserved = reservations;
                Holds = holds;
            }

            public string Name { get; set; }
            public int Quantity { get; set; }
            public int Reserved { get; set; }
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

        [Property(Arbitrary = new[] { typeof(PropertyTests.InventoryArbitrary) })]
        public void Reservation_Test(PropertyTests.Inventory inventory, uint toReserve)
        {
            bool initializationSuccess;
            var inventoryActor = _testHelper.TryInitializeInventoryServiceRepository(inventory, Sys,
                out initializationSuccess);
            if (!initializationSuccess) return;
            var alreadyReserved = inventory.Reserved;
            var r = _testHelper.Reserve(inventoryActor, (int)toReserve, inventory.Name);

            if (inventory.Quantity - inventory.Holds - inventory.Reserved - toReserve >= 0)
            {
                alreadyReserved += (int)toReserve;
                Assert.True(r.Successful);
                Assert.Equal(r.Reserved, alreadyReserved);
            }
            else
            {
                Assert.False(r.Successful);

                Assert.Equal(r.Reserved, 0);
                //-todo when there is a failure, return nothing   Assert.Equal(r.ReservationQuantity, (int)toReserve);
            }
        }

        [Property(Arbitrary = new[] { typeof(PropertyTests.InventoryArbitrary) })]
        public void Purchase_Test(PropertyTests.Inventory inventory, uint toPurchase)
        {
            bool initializationSuccess;
            var inventoryActor = _testHelper.TryInitializeInventoryServiceRepository(inventory, Sys,
                out initializationSuccess);
            if (!initializationSuccess) return;
            var initialQuantity = inventory.Quantity;

            var r = _testHelper.Purchase(inventoryActor, (int)toPurchase, inventory.Name);

            if (inventory.Quantity - inventory.Holds - toPurchase >= 0)
            {
                initialQuantity -= (int)toPurchase;
                Assert.True(r.Successful);
                Assert.Equal(r.Quantity, initialQuantity);
            }
            else
            {
                Assert.False(r.Successful);
                Assert.Equal(r.Quantity, 0);
                //-todo when there is a failure, return nothing      Assert.Equal(r.Quantity, (int)toPurchase);
            }
        }

        [Property(Arbitrary = new[] { typeof(PropertyTests.InventoryArbitrary) })]
        public void Holds_Test(PropertyTests.Inventory inventory, uint toHold)
        {
            bool initializationSuccess;
            var inventoryActor = _testHelper.TryInitializeInventoryServiceRepository(inventory, Sys,
                out initializationSuccess);
            if (!initializationSuccess) return;
            var alreadyHeld = inventory.Holds;

            var r = _testHelper.Hold(inventoryActor, (int)toHold, inventory.Name);

            if (inventory.Quantity - inventory.Holds - toHold - inventory.Reserved >= 0)
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

        [Property(Arbitrary = new[] { typeof(PropertyTests.InventoryArbitrary) })]
        public void UpdateQuantity_Test(PropertyTests.Inventory inventory, int toUpdate)
        {
            bool initializationSuccess;
            var inventoryActor = _testHelper.TryInitializeInventoryServiceRepository(inventory, Sys,
                out initializationSuccess);
            if (!initializationSuccess) return;
            var r = _testHelper.UpdateQuantity(inventoryActor, toUpdate, inventory.Name);

            if (toUpdate < 0)
            {
                Assert.False(r.Successful);
            }
            else
            {
                Assert.True(r.Successful);
            }

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

        [Property(Arbitrary = new[] { typeof(PropertyTests.InventoryArbitrary) })]
        public void PurchaseFromHolds_Test(PropertyTests.Inventory inventory, uint toPurchase)
        {
            bool initializationSuccess;
            var inventoryActor = _testHelper.TryInitializeInventoryServiceRepository(inventory, Sys,
                out initializationSuccess);
            if (!initializationSuccess) return;
            var initialQuantity = inventory.Quantity;

            var r = _testHelper.PurchaseFromHolds(inventoryActor, (int)toPurchase, inventory.Name);

            if (inventory.Holds >= toPurchase && inventory.Quantity - toPurchase >= 0)
            {
                initialQuantity -= (int)toPurchase;
                Assert.True(r.Successful);
                Assert.Equal(r.Quantity, initialQuantity);
            }
            else
            {
                Assert.False(r.Successful);
                Assert.Equal(r.Quantity, 0);
                //-todo when there is a failure, return nothing     Assert.Equal(r.Quantity, (int)toPurchase);
            }
        }

        [Property(Arbitrary = new[] { typeof(PropertyTests.InventoryArbitrary) })]
        public void Reservation_Holds_Purchase_And_PurchaseFromHold_Test(PropertyTests.Inventory inventory, int toUpdate)
        {
            bool initializationSuccess;
            var inventoryActor = _testHelper.TryInitializeInventoryServiceRepository(inventory, Sys, out initializationSuccess);
            if (!initializationSuccess) return;

            var halfQuantity = toUpdate / 2;
            var secondHalfQuantity = toUpdate - halfQuantity;

            Exception finalException = null;
            try
            {
                var updateQuantityresult = _testHelper.UpdateQuantity(inventoryActor, toUpdate, inventory.Name);

                var reservationResult = _testHelper.Reserve(inventoryActor, halfQuantity, inventory.Name);
                var holdsResult = _testHelper.Hold(inventoryActor, secondHalfQuantity, inventory.Name);

                var purchaseResult = _testHelper.Purchase(inventoryActor, halfQuantity, inventory.Name);
                var purchaseFromHoldsResult = _testHelper.PurchaseFromHolds(inventoryActor, secondHalfQuantity, inventory.Name);

                Assert.True(reservationResult.Successful);
                Assert.True(updateQuantityresult.Successful);
                Assert.True(holdsResult.Successful);
                Assert.True(purchaseResult.Successful);
                Assert.True(purchaseFromHoldsResult.Successful);


                var currentInventory = _testHelper.GetInventory(inventoryActor, inventory.Name);
                Assert.Equal(currentInventory.Reserved, inventory.Reserved);
                Assert.Equal(currentInventory.Holds, inventory.Holds);
                Assert.Equal(currentInventory.Quantity, inventory.Quantity);
            }
            catch (Exception e)
            {
                finalException = e;
            }

            if ((toUpdate >= 0 && finalException != null))
            {
                throw finalException;
            }
            if ((toUpdate < 0 && finalException == null))
            {
                throw new Exception("One or more operation must fail if negative update is provided");
            }
        }

        [Property(Arbitrary = new[] { typeof(PropertyTests.InventoryArbitrary) })]
        public void Reservation_Holds_PurchaseFromHold_And_Purchase_Test(PropertyTests.Inventory inventory, int toUpdate)
        {
            bool initializationSuccess;
            var inventoryActor = _testHelper.TryInitializeInventoryServiceRepository(inventory, Sys, out initializationSuccess);
            if (!initializationSuccess) return;

            var halfQuantity = toUpdate / 2;
            var secondHalfQuantity = toUpdate - halfQuantity;

            Exception finalException = null;
            try
            {
                var updateQuantityresult = _testHelper.UpdateQuantity(inventoryActor, toUpdate, inventory.Name);

                var reservationResult = _testHelper.Reserve(inventoryActor, halfQuantity, inventory.Name);
                var holdsResult = _testHelper.Hold(inventoryActor, secondHalfQuantity, inventory.Name);

                var purchaseFromHoldsResult = _testHelper.PurchaseFromHolds(inventoryActor, secondHalfQuantity, inventory.Name);
                var purchaseResult = _testHelper.Purchase(inventoryActor, halfQuantity, inventory.Name);
               

                Assert.True(reservationResult.Successful);
                Assert.True(updateQuantityresult.Successful);
                Assert.True(holdsResult.Successful);
                Assert.True(purchaseResult.Successful);
                Assert.True(purchaseFromHoldsResult.Successful);


                var currentInventory = _testHelper.GetInventory(inventoryActor, inventory.Name);
                Assert.Equal(currentInventory.Reserved, inventory.Reserved);
                Assert.Equal(currentInventory.Holds, inventory.Holds);
                Assert.Equal(currentInventory.Quantity, inventory.Quantity);
            }
            catch (Exception e)
            {
                finalException = e;
            }

            if ((toUpdate >= 0 && finalException != null))
            {
                throw finalException;
            }
            if ((toUpdate < 0 && finalException == null))
            {
                throw new Exception("One or more operation must fail if negative update is provided");
            }
        }
        
        [Property(Arbitrary = new[] { typeof(PropertyTests.InventoryArbitrary) })]
        public void Holds_Reservation_Purchase_And_PurchaseFromHold_Test(PropertyTests.Inventory inventory, int toUpdate)
        {
            bool initializationSuccess;
            var inventoryActor = _testHelper.TryInitializeInventoryServiceRepository(inventory, Sys, out initializationSuccess);
            if (!initializationSuccess) return;

            var halfQuantity = toUpdate / 2;
            var secondHalfQuantity = toUpdate - halfQuantity;

            Exception finalException = null;
            try
            {
                var updateQuantityresult = _testHelper.UpdateQuantity(inventoryActor, toUpdate, inventory.Name);


                var holdsResult = _testHelper.Hold(inventoryActor, secondHalfQuantity, inventory.Name);
                var reservationResult = _testHelper.Reserve(inventoryActor, halfQuantity, inventory.Name);

                var purchaseResult = _testHelper.Purchase(inventoryActor, halfQuantity, inventory.Name);
                var purchaseFromHoldsResult = _testHelper.PurchaseFromHolds(inventoryActor, secondHalfQuantity, inventory.Name);

                Assert.True(reservationResult.Successful);
                Assert.True(updateQuantityresult.Successful);
                Assert.True(holdsResult.Successful);
                Assert.True(purchaseResult.Successful);
                Assert.True(purchaseFromHoldsResult.Successful);

                var currentInventory = _testHelper.GetInventory(inventoryActor, inventory.Name);
                Assert.Equal(currentInventory.Reserved, inventory.Reserved);
                Assert.Equal(currentInventory.Holds, inventory.Holds);
                Assert.Equal(currentInventory.Quantity, inventory.Quantity);
            }
            catch (Exception e)
            {
                finalException = e;
            }

            if ((toUpdate >= 0 && finalException != null))
            {
                throw finalException;
            }
            if ((toUpdate < 0 && finalException == null))
            {
                throw new Exception("One or more operation must fail if negative update is provided");
            }
        }

        [Property(Arbitrary = new[] { typeof(PropertyTests.InventoryArbitrary) })]
        public void Holds_Reservation_PurchaseFromHold_And_Purchase_Test(PropertyTests.Inventory inventory, int toUpdate)
        {
            bool initializationSuccess;
            var inventoryActor = _testHelper.TryInitializeInventoryServiceRepository(inventory, Sys, out initializationSuccess);
            if (!initializationSuccess) return;

            var halfQuantity = toUpdate / 2;
            var secondHalfQuantity = toUpdate - halfQuantity;

            Exception finalException = null;
            try
            {
                var updateQuantityresult = _testHelper.UpdateQuantity(inventoryActor, toUpdate, inventory.Name);

                var holdsResult = _testHelper.Hold(inventoryActor, secondHalfQuantity, inventory.Name);
                var reservationResult = _testHelper.Reserve(inventoryActor, halfQuantity, inventory.Name);

                var purchaseFromHoldsResult = _testHelper.PurchaseFromHolds(inventoryActor, secondHalfQuantity, inventory.Name);
                var purchaseResult = _testHelper.Purchase(inventoryActor, halfQuantity, inventory.Name);

              


                Assert.True(reservationResult.Successful);
                Assert.True(updateQuantityresult.Successful);
                Assert.True(holdsResult.Successful);
                Assert.True(purchaseResult.Successful);
                Assert.True(purchaseFromHoldsResult.Successful);

                var currentInventory = _testHelper.GetInventory(inventoryActor, inventory.Name);
                Assert.Equal(currentInventory.Reserved, inventory.Reserved);
                Assert.Equal(currentInventory.Holds, inventory.Holds);
                Assert.Equal(currentInventory.Quantity, inventory.Quantity);
            }
            catch (Exception e)
            {
                finalException = e;
            }

            if ((toUpdate >= 0 && finalException != null))
            {

                throw finalException;
            }
            if ((toUpdate < 0 && finalException == null))
            {
                throw new Exception("One or more operation must fail if negative update is provided");
            }
        }


        [Property(Arbitrary = new[] { typeof(PropertyTests.InventoryArbitrary) })]
        public void Concurrent_Holds_Reservation_PurchaseFromHold_And_Purchase_Test(PropertyTests.Inventory inventory, int toUpdate, int loopCount)
        {
            bool initializationSuccess;
            var inventoryActor = _testHelper.TryInitializeInventoryServiceRepository(inventory, Sys, out initializationSuccess);
            if (!initializationSuccess) return;

            Parallel.ForEach(Enumerable.Range(0, Math.Abs(loopCount)), (i) =>
            {
                var halfQuantity = toUpdate / 2;
                var secondHalfQuantity = toUpdate - halfQuantity;

                Exception finalException = null;
                UpdateQuantityCompletedMessage updateQuantityresult = null;
                PlaceHoldCompletedMessage holdsResult = null;
                ReserveCompletedMessage reservationResult = null;
                PurchaseFromHoldsCompletedMessage purchaseFromHoldsResult = null;
                PurchaseCompletedMessage purchaseResult = null;
                try
                {
                     updateQuantityresult = _testHelper.UpdateQuantity(inventoryActor, toUpdate, inventory.Name);

                     holdsResult = _testHelper.Hold(inventoryActor, secondHalfQuantity, inventory.Name);
                     reservationResult = _testHelper.Reserve(inventoryActor, halfQuantity, inventory.Name);

                     purchaseFromHoldsResult = _testHelper.PurchaseFromHolds(inventoryActor, secondHalfQuantity, inventory.Name);
                     purchaseResult = _testHelper.Purchase(inventoryActor, halfQuantity, inventory.Name);


                    Assert.True(reservationResult.Successful);
                    Assert.True(updateQuantityresult.Successful);
                    Assert.True(holdsResult.Successful);
                    Assert.True(purchaseResult.Successful);
                    Assert.True(purchaseFromHoldsResult.Successful);

                    var currentInventory = _testHelper.GetInventory(inventoryActor, inventory.Name);
                    Assert.Equal(currentInventory.Reserved, inventory.Reserved);
                    Assert.Equal(currentInventory.Holds, inventory.Holds);
                    Assert.Equal(currentInventory.Quantity, inventory.Quantity);

                    inventory.Quantity += currentInventory.Quantity;
                    inventory.Reserved += currentInventory.Reserved;
                    inventory.Holds += currentInventory.Holds;
                }
                catch (Exception e)
                {
                    finalException = e;
                }

                if ((toUpdate >= 0 && finalException != null))
                {
                   
                    throw finalException;
                }
                if ((toUpdate < 0 && finalException == null))
                {
                    throw new Exception("One or more operation must fail if negative update is provided");
                }
            });





        }
    }
}