using InventoryService.Messages.Response;
using InventoryService.Storage;
using System;
using System.Collections.Generic;
using InventoryService.Messages.Models;

namespace InventoryService.Services
{
    public static class RealTimeInventoryOPerationalResultExtensions
    {
        public static void ThrowIfFailedOperationResult(
            this OperationResult<IRealTimeInventory> realTimeInventoryOperationResult)
        {
            if (realTimeInventoryOperationResult.IsSuccessful) return;
            if (realTimeInventoryOperationResult.Exception != null)
            {
                throw realTimeInventoryOperationResult.Exception;
            }
            var inventoryProductId = realTimeInventoryOperationResult?.Data?.ProductId;
            throw new Exception("Inventory operation failed" + (string.IsNullOrEmpty(inventoryProductId) ? " and did not find inventoryProductId" : inventoryProductId));
        }

        public static OperationResult<IRealTimeInventory> ToSuccessOperationResult(
            this IRealTimeInventory realTimeInventory)
        {
            return new OperationResult<IRealTimeInventory>()
            {
                Data = realTimeInventory,
                IsSuccessful = true
            };
        }

        public static OperationResult<IRealTimeInventory> ToFailedOperationResult(
            this Exception exception, string message = "Inventory operation failed")
        {
            return new OperationResult<IRealTimeInventory>()
            {
                Data = null,
                IsSuccessful = false,
                Exception = new Exception(message, exception)
            };
        }

        public static InventoryOperationErrorMessage ToInventoryOperationErrorMessage(
            this Exception exception, string productId, string message = "Inventory operation failed")
        {
            return new InventoryOperationErrorMessage(productId, new List<Exception>()
            {
                new Exception(message+" - "+exception, exception)
            });
        }

        public static InventoryOperationErrorMessage ToInventoryOperationErrorMessage(
          this OperationResult<IRealTimeInventory> operationResult, string productId, string message = "Inventory operation failed")
        {
            return new InventoryOperationErrorMessage(productId, new List<Exception>()
            {
                new Exception(message+" - "+operationResult.Exception.Message, operationResult.Exception)
            });
        }
    }
}