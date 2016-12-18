namespace InventoryService.Messages.NotificationSubscriptionMessages
{
    public class SubScribeToNotificationCompletedMessage
    {
        public SubScribeToNotificationCompletedMessage(string subscriptionId)
        {
            SubscriptionId = subscriptionId;
        }

        public string SubscriptionId { get; private set; }
    }
}