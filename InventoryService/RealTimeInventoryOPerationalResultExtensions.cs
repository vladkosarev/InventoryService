using Akka.Actor;
using Akka.Event;
using InventoryService.Messages;
using InventoryService.Messages.Models;
using InventoryService.Messages.Response;
using System;

namespace InventoryService
{
    public static class RealTimeInventoryOPerationalResultExtensions
    {
        public static IInventoryServiceCompletedMessage ProcessAndSendResult(this OperationResult<IRealTimeInventory> result, IRequestMessage requestMessage, Func<IRealTimeInventory, IInventoryServiceCompletedMessage> successResponseCompletedMessage, IRealTimeInventory realTimeInventory)
        {
            if (!result.IsSuccessful)
            {
                return result.ToInventoryOperationErrorMessage(requestMessage.ProductId);
            }
            else
            {
                realTimeInventory = result.Data as RealTimeInventory;
                return successResponseCompletedMessage(realTimeInventory);
            }
        }

        public static RealTimeInventory ProcessAndSendResult(this OperationResult<IRealTimeInventory> result, IRequestMessage requestMessage, Func<IRealTimeInventory, IInventoryServiceCompletedMessage> successResponseCompletedMessage, ILoggingAdapter logger, IRealTimeInventory realTimeInventory, IActorRef sender)
        {
            logger.Info(requestMessage.GetType().Name + " Request was " + (!result.IsSuccessful ? " NOT " : "") + " successful.  Current Inventory :  " + realTimeInventory.GetCurrentQuantitiesReport());

            if (!result.IsSuccessful)
            {
                (sender).Tell(result.ToInventoryOperationErrorMessage(requestMessage.ProductId));
                logger.Error("Error while trying to " + requestMessage.GetType() + " - The sender of the message is " + sender.Path, requestMessage, result, realTimeInventory.GetCurrentQuantitiesReport());
            }
            else
            {
                realTimeInventory = result.Data as RealTimeInventory;
                var response = successResponseCompletedMessage(realTimeInventory);
                sender.Tell(response);
                logger.Info(response.GetType().Name + " Response was sent back. Current Inventory : " + realTimeInventory.GetCurrentQuantitiesReport() + " - The sender of the message is " + sender.Path);
            }

            return realTimeInventory as RealTimeInventory;
        }

        public static void ThrowIfFailedOperationResult(
            this OperationResult<IRealTimeInventory> realTimeInventoryOperationResult)
        {
            if (realTimeInventoryOperationResult.IsSuccessful) return;
            if (realTimeInventoryOperationResult.Exception != null)
            {
                throw new Exception(realTimeInventoryOperationResult.Exception.ErrorMessage);
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

        public static string GetCurrentQuantitiesReport(this IRealTimeInventory realTimeInventory)
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
            exception.ErrorMessage += exception.ErrorMessage + " : " + message;
            return new OperationResult<IRealTimeInventory>()
            {
                Data = realTimeInventory,
                IsSuccessful = false,
                Exception = exception
            };
        }

        public static OperationResult<IRealTimeInventory> ToFailedOperationResult(
            this RealTimeInventoryException exception)
        {
            return new OperationResult<IRealTimeInventory>()
            {
                Data = null,
                IsSuccessful = false,
                Exception = exception
            };
        }

        public static InventoryOperationErrorMessage ToInventoryOperationErrorMessage(
            this RealTimeInventoryException exception, string productId)
        {
            return new InventoryOperationErrorMessage(new RealTimeInventory(productId, 0, 0, 0), exception);
        }

        public static InventoryOperationErrorMessage ToInventoryOperationErrorMessage(
          this OperationResult<IRealTimeInventory> operationResult, string productId)
        {
            operationResult.Data = operationResult.Data ?? new RealTimeInventory(productId, 0, 0, 0);

            return new InventoryOperationErrorMessage(operationResult.Data, operationResult.Exception);
        }
    }
}