namespace InventoryService.Messages.Request
{
    public class ServerNotificationMessage
    {
        public ServerNotificationMessage(string serverMessage)
        {
            ServerMessage = serverMessage;
        }

        public string ServerMessage { get; private set; }
    }
}