using System;
using Autofac.Integration.SignalR;
using InventoryService.Messages.Request;
using Microsoft.AspNet.SignalR;

namespace InventoryService.WebUIHost
{
    public class SignalRNotificationService
    {
       
        public void SendInventoryList(QueryInventoryListCompletedMessage inventoryListCompletedMessage)
        {
            GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.inventoryData(inventoryListCompletedMessage);
        }
        public void SendMessageSpeed(double speed)
        {
           
            GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.messageSpeed(speed);
        }
        public void SendServerNotification(string serverNotification)
        {
            GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.serverNotificationMessages(serverNotification);
        }

        public void SendIncomingMessage(string incomingMessage)
        {
            GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.serverNotificationMessages(incomingMessage);
        }
    }
}