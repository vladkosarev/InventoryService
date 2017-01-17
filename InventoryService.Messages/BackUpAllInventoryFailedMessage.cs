namespace InventoryService.Messages
{
    public class BackUpAllInventoryFailedMessage : BackUpAllInventoryCompletedMessage
    {
        public BackUpAllInventoryFailedMessage(string failureMessage)
        {
            FailureMessage = failureMessage;
        }

        public string FailureMessage { get; }
    }
}