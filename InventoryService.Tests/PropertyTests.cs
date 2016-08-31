using Akka.Actor;
using Akka.TestKit.Xunit2;
using FsCheck;
using FsCheck.Xunit;
using InventoryService.Messages.Response;
using InventoryService.Storage.InMemoryLib;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Random = System.Random;

namespace InventoryService.Tests
{
    public class PropertyTests : TestKit
    {
        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Reservation_Test(Inventory inventory, int toReserve)
        {
            var testHelper = new TestHelper(new InMemory());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys,
                out initializationSuccess);
            if (!initializationSuccess) return;
            var initialReserved = inventory.Reserved;
            var r = testHelper.Reserve(inventoryActor, (int)toReserve, inventory.Name);

            AssertReservations(inventory, toReserve, r);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Purchase_Test(Inventory inventory, uint toPurchase)
        {
            var testHelper = new TestHelper(new InMemory());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys,
                out initializationSuccess);
            if (!initializationSuccess) return;
            var initialQuantity = inventory.Quantity;

            var r = testHelper.Purchase(inventoryActor, (int)toPurchase, inventory.Name);

            AssertPurchase(inventory, toPurchase, r);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Holds_Test(Inventory inventory, int toHold)
        {
            var testHelper = new TestHelper(new InMemory());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys,
                out initializationSuccess);
            if (!initializationSuccess) return;
            var initialHeld = inventory.Holds;

            var r = testHelper.Hold(inventoryActor, toHold, inventory.Name);

            AssertHolds(inventory, toHold, r);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void UpadeteQuantityAndHold_Test(Inventory inventory, uint toHold)
        {
            var testHelper = new TestHelper(new InMemory());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys,
                out initializationSuccess);
            if (!initializationSuccess) return;
            var initialHeld = inventory.Holds;

            var r = testHelper.UpdateQuantityAndHold(inventoryActor, (int)toHold, inventory.Name);

            AssertUpdateQuantityAndHold(inventory, toHold, r);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void UpdateQuantity_Test(Inventory inventory, int toUpdate)
        {
            var testHelper = new TestHelper(new InMemory());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys,
                out initializationSuccess);
            if (!initializationSuccess) return;
            var r = testHelper.UpdateQuantity(inventoryActor, toUpdate, inventory.Name);

            AssertUpdateQuantity(inventory, toUpdate, r);
        }

        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void PurchaseFromHolds_Test(Inventory inventory, uint toPurchase)
        {
            var testHelper = new TestHelper(new InMemory());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys,
                out initializationSuccess);
            if (!initializationSuccess) return;
            var initialQuantity = inventory.Quantity;

            var r = testHelper.PurchaseFromHolds(inventoryActor, (int)toPurchase, inventory.Name);

            AssertPurchaseFromHolds(inventory, toPurchase, r);
        }

        [Obsolete("There is initial another test inclusive of this")]
        [Property(Arbitrary = new[] { typeof(InventoryArbitrary) })]
        public void Concurrent_Holds_Reservation_PurchaseFromHold_And_Purchase_Test(Inventory inventory, int toUpdate)
        {
            var testHelper = new TestHelper(new InMemoryDictionary());
            bool initializationSuccess;
            var inventoryActor = testHelper.TryInitializeInventoryServiceRepository(inventory, Sys,
                out initializationSuccess);
            if (!initializationSuccess) return;
            const int looplength = 5;

            var operations = GetOperations(inventory, testHelper, inventoryActor);

            var updatedInventory = inventory;
            for (var i = 0; i < looplength; i++)
            {
                var chooseInt = new Random().Next(1, operations.Count);
                updatedInventory = operations[chooseInt](updatedInventory, toUpdate);
            }
        }

        private static Dictionary<int, Func<Inventory, int, Inventory>> GetOperations(Inventory inventory, TestHelper testHelper, IActorRef inventoryActor)
        {
            var operations = new Dictionary<int, Func<Inventory, int, Inventory>>
            {
                {
                    1, (initialInventory, update) =>
                    {
                        var reservationResult = testHelper.Reserve(inventoryActor, update, inventory.Name);

                        AssertReservations(initialInventory, update, reservationResult);
                        return reservationResult.ToInventory();
                    }
                },
                {
                    2, (initialInventory, update) =>
                    {
                        var reservationResult = testHelper.UpdateQuantity(inventoryActor, update, inventory.Name);

                        AssertUpdateQuantity(initialInventory, update, reservationResult);
                        return reservationResult.ToInventory();
                    }
                },
                {
                    3, (initialInventory, update) =>
                    {
                        var reservationResult = testHelper.Hold(inventoryActor,  update, inventory.Name);

                        AssertHolds(initialInventory, update, reservationResult);
                        return reservationResult.ToInventory();
                    }
                },
                {
                    4, (initialInventory, update) =>
                    {
                        var reservationResult = testHelper.UpdateQuantityAndHold(inventoryActor, update,
                            inventory.Name);

                        AssertUpdateQuantityAndHold(initialInventory,(uint) update, reservationResult);
                        return reservationResult.ToInventory();
                    }
                },
                {
                    5, (initialInventory, update) =>
                    {
                        var reservationResult = testHelper.Purchase(inventoryActor,  update, inventory.Name);

                        AssertPurchase(initialInventory,(uint) update, reservationResult);
                        return reservationResult.ToInventory();
                    }
                },
                {
                    6, (initialInventory, update) =>
                    {
                        var reservationResult = testHelper.PurchaseFromHolds(inventoryActor,  update,
                            inventory.Name);

                        AssertPurchaseFromHolds(initialInventory,(uint) update, reservationResult);
                        return reservationResult.ToInventory();
                    }
                }
            };
            return operations;
        }

        private static void AssertReservations(Inventory initialInventory, int toReserve,
         IInventoryServiceCompletedMessage result)
        {
            if (initialInventory.Quantity - initialInventory.Holds - initialInventory.Reserved - toReserve >= 0)
            {
                Assert.True(result.Successful);
                Assert.Equal(result.Reserved, Math.Max(0,initialInventory.Reserved + toReserve));
            }
            else
            {
                Assert.False(result.Successful);
                Assert.Equal(result.Reserved, initialInventory.Reserved);
            }
            Assert.Equal(result.Holds, initialInventory.Holds);
            Assert.Equal(result.Quantity, initialInventory.Quantity);
        }

        private static void AssertPurchase(Inventory inventory, uint toPurchase, IInventoryServiceCompletedMessage r)
        {
            if ((inventory.Quantity - toPurchase - inventory.Holds >= 0))
            {
                Assert.True(r.Successful);
                Assert.Equal(r.Quantity, inventory.Quantity - (int)toPurchase);
                Assert.Equal(r.Reserved, Math.Max(0, inventory.Reserved - (int)toPurchase));
            }
            else
            {
                Assert.False(r.Successful);
                Assert.Equal(r.Quantity, inventory.Quantity);
                Assert.Equal(r.Reserved, inventory.Reserved);
                //-todo when there is a failure, return nothing      Assert.Equal(r.Quantity, (int)toPurchase);
            }
            Assert.Equal(r.Holds, inventory.Holds);
        }

        private static void AssertHolds(Inventory inventory, int toHold, IInventoryServiceCompletedMessage r)
        {
            if (inventory.Quantity - inventory.Holds - toHold >= 0)
            {
                Assert.True(r.Successful);
                Assert.Equal(r.Holds, inventory.Holds + (int)toHold);
            }
            else
            {
                Assert.False(r.Successful);
                Assert.Equal(r.Holds, inventory.Holds);
                //-todo when there is a failure, return nothing   Assert.Equal(r.Holds, (int)toHold);
            }
            Assert.Equal(r.Reserved, inventory.Reserved);
            Assert.Equal(r.Quantity, inventory.Quantity);
        }

        private static void AssertPurchaseFromHolds(Inventory inventory, uint toPurchase,
            IInventoryServiceCompletedMessage r)
        {
            if ((inventory.Holds >= toPurchase) && (inventory.Quantity - toPurchase >= 0))
            {
                Assert.True(r.Successful);
                Assert.Equal(r.Quantity, inventory.Quantity - (int)toPurchase);
                Assert.Equal(r.Holds, inventory.Holds - (int)toPurchase);
            }
            else
            {
                Assert.False(r.Successful);
                Assert.Equal(r.Quantity, inventory.Quantity);
                Assert.Equal(r.Holds, inventory.Holds);
                //-todo when there is a failure, return nothing      Assert.Equal(r.Quantity, (int)toPurchase);
            }
            Assert.Equal(r.Reserved, inventory.Reserved);
        }

        private static void AssertUpdateQuantity(Inventory inventory, int toUpdate, IInventoryServiceCompletedMessage r)
        {
            Assert.True(r.Successful);
            Assert.Equal(r.Quantity, inventory.Quantity + toUpdate);

            Assert.Equal(r.Reserved, inventory.Reserved);
            Assert.Equal(r.Holds, inventory.Holds);
        }

        private static void AssertUpdateQuantityAndHold(Inventory inventory, uint toUpdate,
            IInventoryServiceCompletedMessage r)
        {
            if (inventory.Holds + toUpdate <= inventory.Quantity)
            {
                Assert.True(r.Successful);
                Assert.Equal(r.Quantity, inventory.Quantity + (int)toUpdate);
                Assert.Equal(r.Reserved, inventory.Reserved);
                Assert.Equal(r.Holds, inventory.Holds + (int)toUpdate);
            }
            else
            {
                Assert.False(r.Successful);
                Assert.Equal(r.Quantity, inventory.Quantity );
                Assert.Equal(r.Reserved, inventory.Reserved);
                Assert.Equal(r.Holds, inventory.Holds );
            }
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
    }
}