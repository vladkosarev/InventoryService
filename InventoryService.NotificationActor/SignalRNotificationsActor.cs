using System;
using Akka.Actor;
using Akka.Event;
using InventoryService.Messages;
using InventoryService.Messages.NotificationSubscriptionMessages;
using InventoryService.Messages.Request;
using NLog.Fluent;

namespace InventoryService.WebUIHost
{
    public class SignalRNotificationsActor : ReceiveActor
    {
        public readonly ILoggingAdapter Logger = Context.GetLogger();
        private SignalRNotificationService SignalRNotificationService { set; get; }

        public SignalRNotificationsActor(string inventoryActorAddress)
        {
            SignalRNotificationService = new SignalRNotificationService();
            Receive<GetMetricsCompletedMessage>(message =>
            {
                SignalRNotificationService.SendMessageSpeed(message.MessageSpeedPersecond);
                Logger.Debug("received  - " + message.GetType().Name + " - MessageSpeedPersecond :" + message.MessageSpeedPersecond);
            });

            Receive<QueryInventoryListCompletedMessage>(message =>
            {
                SignalRNotificationService.SendInventoryList(message);
                Logger.Debug("total inventories in inventory service : " + message?.RealTimeInventories?.Count);
            });

            Receive<IRequestMessage>(message =>
            {
                SignalRNotificationService.SendIncomingMessage(message.GetType().Name + " : " + message.Update + " for " + message.ProductId);
                Logger.Debug("received by inventory Actor - " + message.GetType().Name + " - " + message.ProductId +" : quantity "+message.Update);
            });
            Receive<ServerNotificationMessage>(message =>
            {
                SignalRNotificationService.SendServerNotification(message.ServerMessage);
                Logger.Debug("received  - " + message.GetType().Name + " -  ServerMessage : " + message.ServerMessage);
            });

            var notificationsActor = Context.System.ActorSelection(inventoryActorAddress);
            Logger.Debug("Subscribing for notifications at "+inventoryActorAddress+" ....");
 

            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), notificationsActor, new SubScribeToNotificationMessage(Self),Self);

        }
    }
}