using InventoryService.Messages.Models;
using InventoryService.Messages.Request;
using InventoryService.Messages.Response;
using System;

namespace InventoryService.Messages
{
    public class CompletedMessageFactory
    {
        public static Func<IRealTimeInventory, IInventoryServiceCompletedMessage> GetSuccessResponseCompletedMessage<T>(T message, bool isSuccessful = true) where T : IRequestMessage
        {
            if (message is GetInventoryMessage)
                return (rti) => new GetInventoryCompletedMessage(rti, isSuccessful);

            if (message is ReserveMessage)
                return (rti) => new ReserveCompletedMessage(rti, isSuccessful);

            if (message is UpdateQuantityMessage)
                return (rti) => new UpdateQuantityCompletedMessage(rti, isSuccessful);

            if (message is UpdateAndHoldQuantityMessage)
                return (rti) => new UpdateAndHoldQuantityCompletedMessage(rti, isSuccessful);

            if (message is PlaceHoldMessage)
                return (rti) => new PlaceHoldCompletedMessage(rti, isSuccessful);

            if (message is PurchaseMessage)
                return (rti) => new PurchaseCompletedMessage(rti, isSuccessful);

            if (message is PurchaseFromHoldsMessage)
                return (rti) => new PurchaseFromHoldsCompletedMessage(rti, isSuccessful);

            if (message is ResetInventoryQuantityReserveAndHoldMessage)
                return (rti) => new ResetInventoryQuantityReserveAndHoldCompletedMessage(rti, isSuccessful);

            return null;
        }
    }
}