namespace InventoryService.Messages.NotificationSubscriptionMessages
{
    public class SubScribeToNotificationMessage
    {
        public SubScribeToNotificationMessage(object subscriber)
        {
            Subscriber = subscriber;
        }

        public object Subscriber { get; private set; }
    }
}