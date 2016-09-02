using InventoryService.Messages.Models;
using System;

namespace InventoryService.Messages
{
    public class InventoryServiceErrorMessageGenerator
    {
        public static RealTimeInventoryException Generate(ErrorType errorType, IRealTimeInventory currentRealTimeInventory, int requestUpdate, Exception exceptionThrown = null)
        {
            string result;

            switch (errorType)
            {
                case ErrorType.Unknown:
                    result = "Unknown";
                    break;

                case ErrorType.NO_PRODUCT_ID_SPECIFIED:

                    result = "No product ID specified";

                    break;

                case ErrorType.UNABLE_TO_READ_INV:
                    result = "Unable to read inventory for product " + currentRealTimeInventory.ProductId;

                    break;

                case ErrorType.RESERVATION_EXCEED_QUANTITY:
                    result = "What to reserve must be less than quantity - reservation for product " +
                             currentRealTimeInventory.ProductId;

                    break;

                case ErrorType.UNABLE_TO_UPDATE_INVENTORY_STORAGE:
                    result = "Unable to update inventory storage for product " +
                           currentRealTimeInventory.ProductId + " with quantity " + requestUpdate;

                    break;

                case ErrorType.HOLD_EXCEED_QUANTITY_FOR_HOLD:

                    result = "Unable to  hold With the supplied hold " + requestUpdate +
                             ",the new hold is larger than resulting quantity for product " +
                             currentRealTimeInventory.ProductId;

                    break;

                case ErrorType.HOLD_EXCEED_QUANTITY_FOR_UPDATEQUANTITYANDHOLD:

                    result = "Unable to update quantity and hold With the supplied hold " + requestUpdate +
                             ",the new hold is larger than resulting quantity for product " +
                             currentRealTimeInventory.ProductId;

                    break;

                case ErrorType.NEGATIVE_PURCHASE_FOR_PURCHASEFROMRESERVATION:
                    result = "Cannot purchase from reservation with a negative quantity of " + requestUpdate + " for product " +
                             currentRealTimeInventory.ProductId;

                    break;

                case ErrorType.PURCHASE_EXCEED_QUANTITY_FOR_PURCHASEFROMRESERVATION:
                    result = "provided " + requestUpdate +
                             ", available holds must be less than or equal to quantity for product " +
                             currentRealTimeInventory.ProductId;
                    break;

                case ErrorType.PURCHASE_EXCEED_QUANTITY_FOR_PURCHASEFROMHOLD:
                    result = "provided " + requestUpdate +
                            " for purchase from hold available holds must be less than or equal to quantity for product " +
                            currentRealTimeInventory.ProductId;

                    break;

                case ErrorType.NEGATIVE_PURCHASE_FOR_PURCHASEFROMHOLD:
                    result = "Cannot purchase from hold with a negative quantity of " + requestUpdate + " for product " +
                           currentRealTimeInventory.ProductId;
                    break;

                default:
                    result = "Unknown";
                    break;
            }

            var time = DateTime.UtcNow;
            result = errorType.ToString() + " with requested update quantity " + requestUpdate + " : " + result + (exceptionThrown != null ? " - " + exceptionThrown.Message : "") + " At " + time;
            return new RealTimeInventoryException(result, errorType, time, exceptionThrown);
        }
    }
}