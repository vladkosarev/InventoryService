using Akka.Actor;
using Akka.Event;
using InventoryService.Messages;
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
        private double _messageSpeedPersecond = 0;
        private int _messageCount = 0;

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
#if DEBUG
              Console.WriteLine("total inventories in inventory service : "+   message?.RealTimeInventories?.Count); 
#endif
   });
            Receive<GetMetricsMessage>(message =>
            {
                _messageSpeedPersecond = _messageCount / Seconds;
                GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.messageSpeed(_messageSpeedPersecond);
                GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.serverNotificationMessages(LastReceivedServerMessage);

                _messageSpeedPersecond = 0;
                _messageCount = 0;
            });
            Receive<IRequestMessage>(message =>
            {
                _messageCount++;
                GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.incomingMessage(message.GetType().Name + " : " + message.Update + " for " + message.ProductId);
#if DEBUG
        Console.WriteLine("received by inventory Actor - "+message.GetType().Name+" - "+message);   
#endif
       });
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(Seconds), Self, new GetMetricsMessage(), Self);
        }

        public int Seconds = 5;
    }
}