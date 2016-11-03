namespace InventoryService.Messages.NotificationSubscriptionMessages
{
    public class SubScribeToNotificationFailedMessage
    {
        public SubScribeToNotificationFailedMessage(string reason)
        {
            Reason = reason;
        }

        public string Reason { get; private set; }
    }
}