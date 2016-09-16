using InventoryService.AkkaInMemoryServer;
using InventoryService.Messages;
using InventoryService.Messages.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace InventoryService.Tests
{
    public class InventoryServiceSpecificationHelper
    {
        public static Dictionary<int, Func<IRealTimeInventory, int, IInventoryServiceCompletedMessage, IRealTimeInventory>> GetAssertions()
        {
            var operations = new Dictionary<int, Func<IRealTimeInventory, int, IInventoryServiceCompletedMessage, IRealTimeInventory>>
            {
                {
                    1, (initialInventory, update,reservationResult) =>
                    {
                        AssertReservations(initialInventory, update, reservationResult);
                        return reservationResult.RealTimeInventory;
                    }
                },
                {
                    2, (initialInventory, update,reservationResult) =>
                    {
                        AssertUpdateQuantity(initialInventory, update, reservationResult);
                        return reservationResult.RealTimeInventory;
                    }
                },
                {
                    3, (initialInventory, update,reservationResult) =>
                    {
                        AssertHolds(initialInventory, update, reservationResult);
                        return reservationResult.RealTimeInventory;
                    }
                },
                {
                    4, (initialInventory, update,reservationResult) =>
                    {
                        AssertUpdateQuantityAndHold(initialInventory,(uint) update, reservationResult);
                        return reservationResult.RealTimeInventory;
                    }
                },
                {
                    5, (initialInventory, update,reservationResult) =>
                    {
                        AssertPurchase(initialInventory,(uint) update, reservationResult);
                        return reservationResult.RealTimeInventory;
                    }
                },
                {
                    6, (initialInventory, update,reservationResult) =>
                    {
                        AssertPurchaseFromHolds(initialInventory,(uint) update, reservationResult);
                        return reservationResult.RealTimeInventory;
                    }
                }
            };
            return operations;
        }

        public static Dictionary<int, Func<RealTimeInventory, int, Task<IInventoryServiceCompletedMessage>>> GetOperations(InventoryServiceAkkaInMemory testHelper)
        {
            var operations = new Dictionary<int, Func<RealTimeInventory, int, Task<IInventoryServiceCompletedMessage>>>
            {
                {
                    1, (product, update) =>
                    {
                        var reservationResult = testHelper.ReserveAsync(product, update);
                        return reservationResult;
                    }
                },
                {
                    2, (product, update) =>
                    {
                        var reservationResult = testHelper.UpdateQuantityAsync( product,update);
                        return reservationResult;
                    }
                },
                {
                    3, (product, update) =>
                    {
                        var reservationResult = testHelper.PlaceHoldAsync(product,  update);
                        return reservationResult;
                    }
                },
                {
                    4, (product, update) =>
                    {
                        var reservationResult = testHelper.UpdateQuantityAndHoldAsync(product, update);
                        return reservationResult;
                    }
                },
                {
                    5, (product, update) =>
                    {
                        var reservationResult = testHelper.PurchaseAsync(product,  update);
                        return reservationResult;
                    }
                },
                {
                    6, (product, update) =>
                    {
                        var reservationResult = testHelper.PurchaseFromHoldsAsync(product, update);
                        return reservationResult;
                    }
                }
            };
            return operations;
        }

        public static void AssertReservations(IRealTimeInventory initialInventory, int reservationQuantity, IInventoryServiceCompletedMessage result)
        {
            if ((initialInventory.Quantity - initialInventory.Holds - initialInventory.Reserved - reservationQuantity >= 0) || reservationQuantity <= 0)
            {
                Assert.True(result.Successful);

                var newReserved = Math.Max(0, initialInventory.Reserved + reservationQuantity);

                Assert.Equal(result.RealTimeInventory.Reserved, newReserved);
            }
            else
            {
                Assert.False(result.Successful);
                Assert.Equal(result.RealTimeInventory.Reserved, initialInventory.Reserved);
            }
            Assert.Equal(result.RealTimeInventory.Holds, initialInventory.Holds);
            Assert.Equal(result.RealTimeInventory.Quantity, initialInventory.Quantity);
        }

        public static void AssertPurchase(IRealTimeInventory inventory, uint toPurchase, IInventoryServiceCompletedMessage r)
        {
            if ((inventory.Quantity - toPurchase - inventory.Holds >= 0) && inventory.Quantity >= 0)
            {
                Assert.True(r.Successful);
                Assert.Equal(r.RealTimeInventory.Quantity, inventory.Quantity - (int)toPurchase);
                Assert.Equal(r.RealTimeInventory.Reserved, Math.Max(0, inventory.Reserved - (int)toPurchase));
            }
            else
            {
                Assert.False(r.Successful);
                Assert.Equal(r.RealTimeInventory.Quantity, inventory.Quantity);
                Assert.Equal(r.RealTimeInventory.Reserved, inventory.Reserved);
                //-todo when there is a failure, return nothing      Assert.Equal(r.Quantity, (int)toPurchase);
            }
            Assert.Equal(r.RealTimeInventory.Holds, inventory.Holds);
        }

        public static void AssertHolds(IRealTimeInventory inventory, int toHold, IInventoryServiceCompletedMessage r)
        {
            if (inventory.Quantity - inventory.Holds - toHold >= 0)
            {
                Assert.True(r.Successful);
                Assert.Equal(r.RealTimeInventory.Holds, inventory.Holds + (int)toHold);
            }
            else
            {
                Assert.False(r.Successful);
                Assert.Equal(r.RealTimeInventory.Holds, inventory.Holds);
                //-todo when there is a failure, return nothing   Assert.Equal(r.Holds, (int)toHold);
            }
            Assert.Equal(r.RealTimeInventory.Reserved, inventory.Reserved);
            Assert.Equal(r.RealTimeInventory.Quantity, inventory.Quantity);
        }

        public static void AssertPurchaseFromHolds(IRealTimeInventory inventory, uint toPurchase, IInventoryServiceCompletedMessage r)
        {
            if ((inventory.Holds >= toPurchase) && (inventory.Quantity - toPurchase >= 0))
            {
                Assert.True(r.Successful);
                Assert.Equal(r.RealTimeInventory.Quantity, inventory.Quantity - (int)toPurchase);
                Assert.Equal(r.RealTimeInventory.Holds, inventory.Holds - (int)toPurchase);
            }
            else
            {
                Assert.False(r.Successful);
                Assert.Equal(r.RealTimeInventory.Quantity, inventory.Quantity);
                Assert.Equal(r.RealTimeInventory.Holds, inventory.Holds);
                //-todo when there is a failure, return nothing      Assert.Equal(r.Quantity, (int)toPurchase);
            }
            Assert.Equal(r.RealTimeInventory.Reserved, inventory.Reserved);
        }

        public static void AssertUpdateQuantity(IRealTimeInventory inventory, int toUpdate, IInventoryServiceCompletedMessage r)
        {
            Assert.True(r.Successful);
            Assert.Equal(r.RealTimeInventory.Quantity, inventory.Quantity + toUpdate);

            Assert.Equal(r.RealTimeInventory.Reserved, inventory.Reserved);
            Assert.Equal(r.RealTimeInventory.Holds, inventory.Holds);
        }

        public static void AssertUpdateQuantityAndHold(IRealTimeInventory inventory, uint toUpdate, IInventoryServiceCompletedMessage r)
        {
            if (inventory.Holds <= inventory.Quantity)
            {
                Assert.True(r.Successful);
                Assert.Equal(r.RealTimeInventory.Quantity, inventory.Quantity + (int)toUpdate);
                Assert.Equal(r.RealTimeInventory.Reserved, inventory.Reserved);
                Assert.Equal(r.RealTimeInventory.Holds, inventory.Holds + (int)toUpdate);
            }
            else
            {
                Assert.False(r.Successful);
                Assert.Equal(r.RealTimeInventory.Quantity, inventory.Quantity);
                Assert.Equal(r.RealTimeInventory.Reserved, inventory.Reserved);
                Assert.Equal(r.RealTimeInventory.Holds, inventory.Holds);
            }
        }
    }
}