using InventoryService.Messages.Models;
using InventoryService.Messages.Response;
using System;
using InventoryService.Messages;

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

        private static string GetCurrentQuantitiesReport(IRealTimeInventory realTimeInventory)
        {
            return " [ quantity : " + realTimeInventory.Quantity + " / reservations: " + realTimeInventory.Reserved + " / holds: " + realTimeInventory.Holds + " ]";
        }

        public static OperationResult<IRealTimeInventory> ToFailedOperationResult(
         this RealTimeInventoryException exception, IRealTimeInventory realTimeInventory, string message = null)
        {
            message = message ?? "Inventory operation failed";
            if (realTimeInventory != null)
            {
                message += GetCurrentQuantitiesReport(realTimeInventory);
            }
            return new OperationResult<IRealTimeInventory>()
            {
                Data = realTimeInventory,
                IsSuccessful = false,
                Exception = new Exception(message, exception)
            };
        }

        public static OperationResult<IRealTimeInventory> ToFailedOperationResult(
            this RealTimeInventoryException exception, string message = "Inventory operation failed")
        {
            return new OperationResult<IRealTimeInventory>()
            {
                Data = null,
                IsSuccessful = false,
                Exception = new Exception(message, exception)
            };
        }

        public static InventoryOperationErrorMessage ToInventoryOperationErrorMessage(
            this RealTimeInventoryException exception, string productId, string message = "Inventory operation failed")
        {
            return new InventoryOperationErrorMessage(new RealTimeInventory(productId,0,0,0), new AggregateException(new Exception(message + " - " + exception, exception)));
        }

        public static InventoryOperationErrorMessage ToInventoryOperationErrorMessage(
          this OperationResult<IRealTimeInventory> operationResult, string productId, string message = "Inventory operation failed")
        {
            operationResult.Data = operationResult.Data ?? new RealTimeInventory(productId, 0, 0, 0);

            var exceptionMessage = operationResult?.Exception?.InnerException?.Message;

            return new InventoryOperationErrorMessage(operationResult.Data, new AggregateException(exceptionMessage, new Exception(message, operationResult.Exception)));
        }

      
    }
}