namespace InventoryService.Messages.NotificationSubscriptionMessages
{
    public class CheckIfNotificationSubscriptionExistsCompletedMessage
    {
        public CheckIfNotificationSubscriptionExistsCompletedMessage(bool isSubscribed)
        {
            IsSubscribed = isSubscribed;
        }

        public bool IsSubscribed { get; private set; }
    }
}