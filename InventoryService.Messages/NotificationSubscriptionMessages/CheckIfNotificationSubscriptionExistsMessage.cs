using System;

namespace InventoryService.Messages.NotificationSubscriptionMessages
{
    public class CheckIfNotificationSubscriptionExistsMessage
    {
        public CheckIfNotificationSubscriptionExistsMessage(string subscriptionId)
        {
            SubscriptionId = subscriptionId;
        }

        public string SubscriptionId { get; private set; }
    }
}