using System;

namespace InventoryService.Messages
{
    public class ActorAliveMessage
    {
        public ActorAliveMessage(Guid subscriptionId)
        {
            SubscriptionId = subscriptionId;
        }

        public Guid SubscriptionId { get; private set; }
    }
}