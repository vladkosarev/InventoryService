using System;

namespace InventoryService.Messages.NotificationSubscriptionMessages
{
    public class CheckIfNotificationSubscriptionExistsMessage
    {
        private CheckIfNotificationSubscriptionExistsMessage(Guid subscriptionId)
        {
            SubscriptionId = subscriptionId;
        }

        public Guid SubscriptionId { get; private set; }
    }
}