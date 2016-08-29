using System;
using System.Collections.Generic;
using InventoryService.Messages.Response;
using InventoryService.Storage;

namespace InventoryService.Services
{
    public static class RealTimeInventoryOPerationalResultExtensions
    {
        public static void ThrowIfFailedOperationResult(
            this OperationResult<RealTimeInventory> realTimeInventoryOperationResult)
        {
            if (realTimeInventoryOperationResult.IsSuccessful) return;
            if (realTimeInventoryOperationResult.Exception != null)
            {
                throw realTimeInventoryOperationResult.Exception;
            }
            var inventoryProductId = realTimeInventoryOperationResult?.Result?.ProductId;
            throw new Exception("Inventory operation failed" + (string.IsNullOrEmpty(inventoryProductId) ? "" : inventoryProductId));
        }

        public static OperationResult<RealTimeInventory> ToSuccessOperationResult(
            this RealTimeInventory realTimeInventory)
        {
            return new OperationResult<RealTimeInventory>()
            {
                Result = realTimeInventory,
                IsSuccessful = true
            };
        }

        public static OperationResult<RealTimeInventory> ToFailedOperationResult(
            this Exception exception, string message = "Inventory operation failed")
        {
            return new OperationResult<RealTimeInventory>()
            {
                Result = null,
                IsSuccessful = false,
                Exception = new Exception(message, exception)
            };
        }

        public static InventoryOperationErrorMessage ToInventoryOperationErrorMessage(
            this Exception exception, string productId, string message = "Inventory operation failed")
        {
            return new InventoryOperationErrorMessage(productId, new List<Exception>()
            {
                new Exception(message, exception)
            });
        }
    }
}