namespace InventoryService.Messages
{
    public class ActorAliveMessage
    {
        public ActorAliveMessage(string subscriptionId)
        {
            SubscriptionId = subscriptionId;
        }

        public string SubscriptionId { get; private set; }
    }
}