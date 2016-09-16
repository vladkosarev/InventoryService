using Akka.Actor;
using Akka.Event;
using InventoryService.Messages.Request;
using Microsoft.AspNet.SignalR;
using System;

namespace InventoryService.NotificationActor
{
    public class NotificationsActor : ReceiveActor
    {
        public readonly ILoggingAdapter Logger = Context.GetLogger();
        private QueryInventoryListCompletedMessage LastReceivedInventoryListMessage { set; get; }
        private string LastReceivedServerMessage { set; get; }

        public NotificationsActor()
        {
            LastReceivedServerMessage = "System started at " + DateTime.UtcNow;

            Receive<string>(message =>
            {
                LastReceivedServerMessage = string.IsNullOrEmpty(message) ? LastReceivedServerMessage : message;
                GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.serverNotificationMessages(LastReceivedServerMessage);
            });

            Receive<QueryInventoryListMessage>(message =>
          {
              GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.inventoryData(LastReceivedInventoryListMessage);
          });

            Receive<QueryInventoryListCompletedMessage>(message =>
            {
                LastReceivedInventoryListMessage = message;
                GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.inventoryData(message);
            });
        }
    }
}