using System;

namespace InventoryService.Messages.NotificationSubscriptionMessages
{
    public class UnSubScribeToNotificationMessage
    {
        public UnSubScribeToNotificationMessage(Guid subscriptionId)
        {
            SubscriptionId = subscriptionId;
        }

        public Guid SubscriptionId { get; private set; }
    }
}