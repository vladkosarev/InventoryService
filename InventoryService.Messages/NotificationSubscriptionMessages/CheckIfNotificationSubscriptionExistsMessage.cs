using System;

namespace InventoryService.Messages.NotificationSubscriptionMessages
{
    public class CheckIfNotificationSubscriptionExistsMessage
    {
        CheckIfNotificationSubscriptionExistsMessage(Guid subscriptionId)
        {
            SubscriptionId = subscriptionId;
        }

        public Guid SubscriptionId { get; private set; }
    }
}