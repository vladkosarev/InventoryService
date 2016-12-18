namespace InventoryService.Messages
{
    public class GetMetricsCompletedMessage
    {
        public GetMetricsCompletedMessage(double messageSpeedPersecond)
        {
            MessageSpeedPersecond = messageSpeedPersecond;
        }

        public double MessageSpeedPersecond { get; private set; }
    }
}