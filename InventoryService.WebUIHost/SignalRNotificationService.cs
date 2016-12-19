using InventoryService.Messages.Request;
using Microsoft.AspNet.SignalR;
using System.Collections.Generic;

namespace InventoryService.WebUIHost
{
    public class SignalRNotificationService
    {
        public void SendInventoryList(QueryInventoryCompletedMessage inventoryListCompletedMessage, List<string> operationNames)
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
        //todo refactor so it does not send to every one!
        public void SendInventoryExportCsv(string csv)
        {
            GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.inventoryExportCsv(csv);
        }

        public void SendIncomingMessage(string incomingMessage)
        {
            GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.incomingMessage(incomingMessage);
        }
    }
}