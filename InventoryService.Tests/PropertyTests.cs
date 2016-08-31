using Akka.TestKit.Xunit2;
using FsCheck;
using FsCheck.Xunit;
using InventoryService.Messages.Response;
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
            var testHelper = new TestHelper(new InMemory());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys,
                out initializationSuccess);
            if (!initializationSuccess) return;
            var alreadyReserved = inventory.Reserved;
            var r = testHelper.Reserve(inventoryActor, (int)toReserve, inventory.Name);

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
            var testHelper = new TestHelper(new InMemory());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys,
                out initializationSuccess);
            if (!initializationSuccess) return;
            var initialQuantity = inventory.Quantity;

            var r = testHelper.Purchase(inventoryActor, (int)toPurchase, inventory.Name);

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
            var testHelper = new TestHelper(new InMemory());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys,
                out initializationSuccess);
            if (!initializationSuccess) return;
            var alreadyHeld = inventory.Holds;

            var r = testHelper.Hold(inventoryActor, (int)toHold, inventory.Name);

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
        public void UpadeteQuantityAndHold_Test(PropertyTests.Inventory inventory, uint toHold)
        {
            var testHelper = new TestHelper(new InMemory());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys,
                out initializationSuccess);
            if (!initializationSuccess) return;
            var alreadyHeld = inventory.Holds;

            var r = testHelper.UpdateQuantityAndHold(inventoryActor, (int)toHold, inventory.Name);

            if (inventory.Quantity + toHold - inventory.Holds - toHold - inventory.Reserved >= 0)
            {
                var newHold = (int)toHold + alreadyHeld;
                Assert.True(r.Successful);
                Assert.Equal(r.Holds, newHold);
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
            var testHelper = new TestHelper(new InMemory());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys,
                out initializationSuccess);
            if (!initializationSuccess) return;
            var r = testHelper.UpdateQuantity(inventoryActor, toUpdate, inventory.Name);

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
            var testHelper = new TestHelper(new InMemory());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys,
                out initializationSuccess);
            if (!initializationSuccess) return;
            var initialQuantity = inventory.Quantity;

            var r = testHelper.PurchaseFromHolds(inventoryActor, (int)toPurchase, inventory.Name);

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

        [Obsolete("There is already another test inclusive of this")]
        [Property(Arbitrary = new[] { typeof(PropertyTests.InventoryArbitrary) })]
        public void Reservation_Holds_Purchase_And_PurchaseFromHold_Test(PropertyTests.Inventory inventory, int toUpdate)
        {
            var testHelper = new TestHelper(new InMemory());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys, out initializationSuccess);
            if (!initializationSuccess) return;

            var halfQuantity = toUpdate / 2;
            var secondHalfQuantity = toUpdate - halfQuantity;

            Exception finalException = null;
            try
            {
                var updateQuantityresult = testHelper.UpdateQuantity(inventoryActor, toUpdate, inventory.Name);

                var reservationResult = testHelper.Reserve(inventoryActor, halfQuantity, inventory.Name);
                var holdsResult = testHelper.Hold(inventoryActor, secondHalfQuantity, inventory.Name);

                var purchaseResult = testHelper.Purchase(inventoryActor, halfQuantity, inventory.Name);
                var purchaseFromHoldsResult = testHelper.PurchaseFromHolds(inventoryActor, secondHalfQuantity, inventory.Name);

                Assert.True(reservationResult.Successful);
                Assert.True(updateQuantityresult.Successful);
                Assert.True(holdsResult.Successful);
                Assert.True(purchaseResult.Successful);
                Assert.True(purchaseFromHoldsResult.Successful);

                var currentInventory = testHelper.GetInventory(inventoryActor, inventory.Name);
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

        [Obsolete("There is already another test inclusive of this")]
        [Property(Arbitrary = new[] { typeof(PropertyTests.InventoryArbitrary) })]
        public void Reservation_Holds_PurchaseFromHold_And_Purchase_Test(PropertyTests.Inventory inventory, int toUpdate)
        {
            var testHelper = new TestHelper(new InMemory());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys, out initializationSuccess);
            if (!initializationSuccess) return;

            var halfQuantity = toUpdate / 2;
            var secondHalfQuantity = toUpdate - halfQuantity;

            Exception finalException = null;
            try
            {
                var updateQuantityresult = testHelper.UpdateQuantity(inventoryActor, toUpdate, inventory.Name);

                var reservationResult = testHelper.Reserve(inventoryActor, halfQuantity, inventory.Name);
                var holdsResult = testHelper.Hold(inventoryActor, secondHalfQuantity, inventory.Name);

                var purchaseFromHoldsResult = testHelper.PurchaseFromHolds(inventoryActor, secondHalfQuantity, inventory.Name);
                var purchaseResult = testHelper.Purchase(inventoryActor, halfQuantity, inventory.Name);

                Assert.True(reservationResult.Successful);
                Assert.True(updateQuantityresult.Successful);
                Assert.True(holdsResult.Successful);
                Assert.True(purchaseResult.Successful);
                Assert.True(purchaseFromHoldsResult.Successful);

                var currentInventory = testHelper.GetInventory(inventoryActor, inventory.Name);
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

        [Obsolete("There is already another test inclusive of this")]
        [Property(Arbitrary = new[] { typeof(PropertyTests.InventoryArbitrary) })]
        public void Holds_Reservation_Purchase_And_PurchaseFromHold_Test(PropertyTests.Inventory inventory, int toUpdate)
        {
            var testHelper = new TestHelper(new InMemory());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys, out initializationSuccess);
            if (!initializationSuccess) return;

            var halfQuantity = toUpdate / 2;
            var secondHalfQuantity = toUpdate - halfQuantity;

            Exception finalException = null;
            try
            {
                var updateQuantityresult = testHelper.UpdateQuantity(inventoryActor, toUpdate, inventory.Name);

                var holdsResult = testHelper.Hold(inventoryActor, secondHalfQuantity, inventory.Name);
                var reservationResult = testHelper.Reserve(inventoryActor, halfQuantity, inventory.Name);

                var purchaseResult = testHelper.Purchase(inventoryActor, halfQuantity, inventory.Name);
                var purchaseFromHoldsResult = testHelper.PurchaseFromHolds(inventoryActor, secondHalfQuantity, inventory.Name);

                Assert.True(reservationResult.Successful);
                Assert.True(updateQuantityresult.Successful);
                Assert.True(holdsResult.Successful);
                Assert.True(purchaseResult.Successful);
                Assert.True(purchaseFromHoldsResult.Successful);

                var currentInventory = testHelper.GetInventory(inventoryActor, inventory.Name);
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

        [Obsolete("There is already another test inclusive of this")]
        [Property(Arbitrary = new[] { typeof(PropertyTests.InventoryArbitrary) })]
        public void Holds_Reservation_PurchaseFromHold_And_Purchase_Test(PropertyTests.Inventory inventory, int toUpdate)
        {
            var testHelper = new TestHelper(new InMemory());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys, out initializationSuccess);
            if (!initializationSuccess) return;

            var halfQuantity = toUpdate / 2;
            var secondHalfQuantity = toUpdate - halfQuantity;

            Exception finalException = null;
            try
            {
                var updateQuantityresult = testHelper.UpdateQuantity(inventoryActor, toUpdate, inventory.Name);

                var holdsResult = testHelper.Hold(inventoryActor, secondHalfQuantity, inventory.Name);
                var reservationResult = testHelper.Reserve(inventoryActor, halfQuantity, inventory.Name);

                var purchaseFromHoldsResult = testHelper.PurchaseFromHolds(inventoryActor, secondHalfQuantity, inventory.Name);
                var purchaseResult = testHelper.Purchase(inventoryActor, halfQuantity, inventory.Name);

                Assert.True(reservationResult.Successful);
                Assert.True(updateQuantityresult.Successful);
                Assert.True(holdsResult.Successful);
                Assert.True(purchaseResult.Successful);
                Assert.True(purchaseFromHoldsResult.Successful);

                var currentInventory = testHelper.GetInventory(inventoryActor, inventory.Name);
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

        [Obsolete("There is already another test inclusive of this")]
        [Property(Arbitrary = new[] { typeof(PropertyTests.InventoryArbitrary) })]
        public void Concurrent_Holds_Reservation_PurchaseFromHold_And_Purchase_Test(PropertyTests.Inventory inventory, uint toUpdate, int loopCount)
        {
            var testHelper = new TestHelper(new InMemoryDictionary());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys, out initializationSuccess);
            if (!initializationSuccess) return;
            IInventoryServiceCompletedMessage currentInventory = null;
            Parallel.ForEach(Enumerable.Range(0, Math.Abs(loopCount) * 10), (i) =>
              {
                  var halfQuantity = toUpdate / 2;
                  var secondHalfQuantity = toUpdate - halfQuantity;

                  Exception finalException = null;
                  IInventoryServiceCompletedMessage updateQuantityresult = null;
                  IInventoryServiceCompletedMessage holdsResult = null;
                  IInventoryServiceCompletedMessage reservationResult = null;
                  IInventoryServiceCompletedMessage purchaseFromHoldsResult = null;
                  IInventoryServiceCompletedMessage purchaseResult = null;

                  try
                  {
                      updateQuantityresult = testHelper.UpdateQuantity(inventoryActor, (int)toUpdate, inventory.Name);

                      holdsResult = testHelper.Hold(inventoryActor, (int)secondHalfQuantity, inventory.Name);
                      reservationResult = testHelper.Reserve(inventoryActor, (int)halfQuantity, inventory.Name);

                      purchaseFromHoldsResult = testHelper.PurchaseFromHolds(inventoryActor, (int)secondHalfQuantity, inventory.Name);
                      purchaseResult = testHelper.Purchase(inventoryActor, (int)halfQuantity, inventory.Name);

                      Assert.True(reservationResult.Successful);
                      Assert.True(updateQuantityresult.Successful);
                      Assert.True(holdsResult.Successful);
                      Assert.True(purchaseResult.Successful);
                      Assert.True(purchaseFromHoldsResult.Successful);
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

            currentInventory = testHelper.GetInventory(inventoryActor, inventory.Name);

            Assert.Equal(currentInventory.Reserved, inventory.Reserved);
            Assert.Equal(currentInventory.Holds, inventory.Holds);
            Assert.Equal(currentInventory.Quantity, inventory.Quantity);
        }

        [Property(Arbitrary = new[] { typeof(PropertyTests.InventoryArbitrary) })]
        public void Holds_Reservation_PurchaseFromHold_And_Purchase_Combination_Test(PropertyTests.Inventory inventory, uint toUpdate, uint executionStrategy)
        {
            var testHelper = new TestHelper(new InMemory());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys, out initializationSuccess);
            if (!initializationSuccess) return;

            var halfQuantity = toUpdate / 2;
            var secondHalfQuantity = toUpdate - halfQuantity;

            Exception finalException = null;
            try
            {
                var updateQuantityresult = testHelper.UpdateQuantity(inventoryActor, (int)toUpdate, inventory.Name);
                Assert.True(updateQuantityresult.Successful);

                var strategy = executionStrategy % 4;

                IInventoryServiceCompletedMessage holdsResult = null;
                IInventoryServiceCompletedMessage reservationResult = null;
                IInventoryServiceCompletedMessage purchaseFromHoldsResult = null;
                IInventoryServiceCompletedMessage purchaseResult = null;
                switch (strategy)
                {
                    case 0:
                        holdsResult = testHelper.Hold(inventoryActor, (int)secondHalfQuantity, inventory.Name);
                        reservationResult = testHelper.Reserve(inventoryActor, (int)halfQuantity, inventory.Name);

                        purchaseFromHoldsResult = testHelper.PurchaseFromHolds(inventoryActor, (int)secondHalfQuantity, inventory.Name);
                        purchaseResult = testHelper.Purchase(inventoryActor, (int)halfQuantity, inventory.Name);
                        break;

                    case 1:
                        reservationResult = testHelper.Reserve(inventoryActor, (int)halfQuantity, inventory.Name);
                        holdsResult = testHelper.Hold(inventoryActor, (int)secondHalfQuantity, inventory.Name);

                        purchaseFromHoldsResult = testHelper.PurchaseFromHolds(inventoryActor, (int)secondHalfQuantity, inventory.Name);
                        purchaseResult = testHelper.Purchase(inventoryActor, (int)halfQuantity, inventory.Name);
                        break;

                    case 2:
                        holdsResult = testHelper.Hold(inventoryActor, (int)secondHalfQuantity, inventory.Name);
                        reservationResult = testHelper.Reserve(inventoryActor, (int)halfQuantity, inventory.Name);

                        purchaseResult = testHelper.Purchase(inventoryActor, (int)halfQuantity, inventory.Name);
                        purchaseFromHoldsResult = testHelper.PurchaseFromHolds(inventoryActor, (int)secondHalfQuantity, inventory.Name);

                        break;

                    case 3:
                        reservationResult = testHelper.Reserve(inventoryActor, (int)halfQuantity, inventory.Name);
                        holdsResult = testHelper.Hold(inventoryActor, (int)secondHalfQuantity, inventory.Name);

                        purchaseResult = testHelper.Purchase(inventoryActor, (int)halfQuantity, inventory.Name);
                        purchaseFromHoldsResult = testHelper.PurchaseFromHolds(inventoryActor, (int)secondHalfQuantity, inventory.Name);

                        break;
                }

                Assert.True(reservationResult.Successful);
                Assert.True(holdsResult.Successful);
                Assert.True(purchaseResult.Successful);
                Assert.True(purchaseFromHoldsResult.Successful);

                var currentInventory = testHelper.GetInventory(inventoryActor, inventory.Name);
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
        public void Concurrent_Holds_Reservation_PurchaseFromHold_And_Purchase_Combination_Test(PropertyTests.Inventory inventory, uint toUpdate, uint executionStrategy, int loopCount)
        {
            var testHelper = new TestHelper(new InMemory());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys, out initializationSuccess);
            if (!initializationSuccess) return;

            Exception finalException = null;
            try
            {
                Parallel.ForEach(Enumerable.Range(0, Math.Abs(loopCount) * 10), (i) =>
                {
                    var updateQuantityresult = testHelper.UpdateQuantity(inventoryActor, (int)toUpdate, inventory.Name);
                    Assert.True(updateQuantityresult.Successful);
                    var halfQuantity = toUpdate / 2;
                    var secondHalfQuantity = toUpdate - halfQuantity;
                    var strategy = executionStrategy % 4;

                    IInventoryServiceCompletedMessage holdsResult = null;
                    IInventoryServiceCompletedMessage reservationResult = null;
                    IInventoryServiceCompletedMessage purchaseFromHoldsResult = null;
                    IInventoryServiceCompletedMessage purchaseResult = null;

                    switch (strategy)
                    {
                        case 0:
                            holdsResult = testHelper.Hold(inventoryActor, (int)secondHalfQuantity, inventory.Name);
                            reservationResult = testHelper.Reserve(inventoryActor, (int)halfQuantity, inventory.Name);

                            purchaseFromHoldsResult = testHelper.PurchaseFromHolds(inventoryActor, (int)secondHalfQuantity,
                                inventory.Name);
                            purchaseResult = testHelper.Purchase(inventoryActor, (int)halfQuantity, inventory.Name);
                            break;

                        case 1:
                            reservationResult = testHelper.Reserve(inventoryActor, (int)halfQuantity, inventory.Name);
                            holdsResult = testHelper.Hold(inventoryActor, (int)secondHalfQuantity, inventory.Name);

                            purchaseFromHoldsResult = testHelper.PurchaseFromHolds(inventoryActor, (int)secondHalfQuantity,
                                inventory.Name);
                            purchaseResult = testHelper.Purchase(inventoryActor, (int)halfQuantity, inventory.Name);
                            break;

                        case 2:
                            holdsResult = testHelper.Hold(inventoryActor, (int)secondHalfQuantity, inventory.Name);
                            reservationResult = testHelper.Reserve(inventoryActor, (int)halfQuantity, inventory.Name);

                            purchaseResult = testHelper.Purchase(inventoryActor, (int)halfQuantity, inventory.Name);
                            purchaseFromHoldsResult = testHelper.PurchaseFromHolds(inventoryActor, (int)secondHalfQuantity,
                                inventory.Name);

                            break;

                        case 3:
                            reservationResult = testHelper.Reserve(inventoryActor, (int)halfQuantity, inventory.Name);
                            holdsResult = testHelper.Hold(inventoryActor, (int)secondHalfQuantity, inventory.Name);

                            purchaseResult = testHelper.Purchase(inventoryActor, (int)halfQuantity, inventory.Name);
                            purchaseFromHoldsResult = testHelper.PurchaseFromHolds(inventoryActor, (int)secondHalfQuantity,
                                inventory.Name);

                            break;
                    }
                    Assert.True(reservationResult.Successful);
                    Assert.True(holdsResult.Successful);
                    Assert.True(purchaseResult.Successful);
                    Assert.True(purchaseFromHoldsResult.Successful);
                });

                var currentInventory = testHelper.GetInventory(inventoryActor, inventory.Name);
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
    }
}