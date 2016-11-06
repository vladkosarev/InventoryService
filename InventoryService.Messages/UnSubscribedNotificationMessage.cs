using System;

namespace InventoryService.Messages
{
    public class UnSubscribedNotificationMessage
    {
        public UnSubscribedNotificationMessage(Guid subscriptionId)
        {
            SubscriptionId = subscriptionId;
        }

        public Guid SubscriptionId { get; private set; }
    }
}