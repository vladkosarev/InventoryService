using System;

namespace InventoryService.Messages
{
    public class UnSubscribedNotificationMessage
    {
        public UnSubscribedNotificationMessage(string subscriptionId)
        {
            SubscriptionId = subscriptionId;
        }

        public string SubscriptionId { get; private set; }
    }
}