using Akka.Actor;
using Akka.Event;
using InventoryService.Messages.Request;
using Microsoft.AspNet.SignalR;
using System;
using InventoryService.Messages;

namespace InventoryService.NotificationActor
{
    public class NotificationsActor : ReceiveActor
    {
        public readonly ILoggingAdapter Logger = Context.GetLogger();
        private QueryInventoryListCompletedMessage LastReceivedInventoryListMessage { set; get; }
        private string LastReceivedServerMessage { set; get; }
        private double messageSpeedPersecond = 0;
        private int messageCount = 0;
        public NotificationsActor()
        {
            LastReceivedServerMessage = "System started at " + DateTime.UtcNow;

            Receive<string>(message =>
            {
                LastReceivedServerMessage = string.IsNullOrEmpty(message) ? LastReceivedServerMessage : message;
               
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
            Receive<GetMetricsMessage>(message =>
            {
            
                 messageSpeedPersecond = messageCount/Seconds;
               GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.messageSpeed(messageSpeedPersecond);
                GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.serverNotificationMessages(LastReceivedServerMessage);
                  messageSpeedPersecond = 0;
                  messageCount = 0;

             
            });
            Receive<IRequestMessage>(message =>
            {
                messageCount++;
                GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.incomingMessage(message.GetType().Name + " : " +message.Update+ " for "+message.ProductId);
            });
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(Seconds), Self, new GetMetricsMessage(), Self);
        }

        public int Seconds = 5;
    }
}