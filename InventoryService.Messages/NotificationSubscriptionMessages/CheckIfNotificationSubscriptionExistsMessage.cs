using System;

namespace InventoryService.Messages.NotificationSubscriptionMessages
{
    public class CheckIfNotificationSubscriptionExistsMessage
    {
        public CheckIfNotificationSubscriptionExistsMessage(Guid subscriptionId)
        {
            SubscriptionId = subscriptionId;
        }

        public Guid SubscriptionId { get; private set; }
    }
}