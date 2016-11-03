using System;

namespace InventoryService.Messages.NotificationSubscriptionMessages
{
    public class SubScribeToNotificationCompletedMessage
    {
        public SubScribeToNotificationCompletedMessage(Guid subscriptionId)
        {
            SubscriptionId = subscriptionId;
        }

        public Guid SubscriptionId { get; private set; }
    }
}