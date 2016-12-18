namespace InventoryService.Messages.NotificationSubscriptionMessages
{
    public class UnSubScribeToNotificationMessage
    {
        public UnSubScribeToNotificationMessage(string subscriptionId)
        {
            SubscriptionId = subscriptionId;
        }

        public string SubscriptionId { get; private set; }
    }
}