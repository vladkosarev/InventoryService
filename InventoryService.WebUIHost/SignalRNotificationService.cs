using System.Collections.Generic;
using InventoryService.Messages.Request;
using Microsoft.AspNet.SignalR;

namespace InventoryService.WebUIHost
{
    public class SignalRNotificationService
    {
        public void SendInventoryList(QueryInventoryListCompletedMessage inventoryListCompletedMessage,List<string> operationNames)
        {
            GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.inventoryData(inventoryListCompletedMessage);
            GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.operationNames(operationNames);
        }

       

        public void SendMessageSpeed(double speed)
        {
            GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.messageSpeed(speed);
        }

        public void SendServerNotification(string serverNotification)
        {
            GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.serverNotificationMessages(serverNotification);
        }
        public void SendJsonResultNotification(string json)
        {
            GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.jsonNotificationMessages(json);
        }
        public void SendIncomingMessage(string incomingMessage)
        {
            GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.incomingMessage(incomingMessage);
        }
    }
}