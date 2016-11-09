using Akka.Actor;
using Akka.Event;
using InventoryService.Messages;
using InventoryService.Messages.NotificationSubscriptionMessages;
using InventoryService.Messages.Request;
using System;
using System.Threading.Tasks;

namespace InventoryService.WebUIHost
{
    public class SignalRNotificationsActor : ReceiveActor
    {
        public readonly ILoggingAdapter Logger = Context.GetLogger();
        private SignalRNotificationService SignalRNotificationService { set; get; }

        private string SubscriptionId { set; get; }

        public SignalRNotificationsActor(string inventoryActorAddress)
        {
            SignalRNotificationService = new SignalRNotificationService();

            Receive<GetMetricsCompletedMessage>(message =>
            {
                Console.WriteLine(Sender.Path);
                SignalRNotificationService.SendMessageSpeed(message.MessageSpeedPersecond);
                Logger.Debug("received  - " + message.GetType().Name + " - MessageSpeedPersecond :" + message.MessageSpeedPersecond);
            });

            Receive<QueryInventoryListCompletedMessage>(message =>
            {
                Console.WriteLine(Sender.Path);
                SignalRNotificationService.SendInventoryList(message);
                Logger.Debug("total inventories in inventory service : " + message?.RealTimeInventories?.Count);
            });

            Receive<IRequestMessage>(message =>
            {
                Console.WriteLine(Sender.Path);
                SignalRNotificationService.SendIncomingMessage(message.GetType().Name + " : " + message.Update + " for " + message.ProductId);
                Logger.Debug("received by inventory Actor - " + message.GetType().Name + " - " + message.ProductId + " : quantity " + message.Update);
            });

            Receive<ServerNotificationMessage>(message =>
            {
                Console.WriteLine(Sender.Path);
                SignalRNotificationService.SendServerNotification(message.ServerMessage);
                Logger.Debug("received  - " + message.GetType().Name + " -  ServerMessage : " + message.ServerMessage);
            });

            Receive<GetAllInventoryListMessage>(message =>
            {
                if (NotificationsActorRef != null && !NotificationsActorRef.IsNobody())
                {
                    NotificationsActorRef.Tell(message);
                }
                else
                {
                    Self.Tell(new SubscribeToNotifications());
                }
            });

            ReceiveAsync<UnSubscribedNotificationMessage>(async _ =>
            {
                Logger.Error("Suddenly unsubscribed to notification. ");
                NotificationsActorRef = await SubscribeToRemoteNotificationActorMessasges(inventoryActorAddress);
            });
            ReceiveAsync<SubscribeToNotifications>(async _ =>
            {
                NotificationsActorRef = await SubscribeToRemoteNotificationActorMessasges(inventoryActorAddress);
            });

            Receive<SubScribeToNotificationCompletedMessage>(message =>
            {
                SubscriptionId = message.SubscriptionId;
                Context.Watch(NotificationsActorRef);
            });


            //Receive<MonitorHealthMessage>(message =>
            //{
            //    Context.System.ActorSelection(inventoryActorAddress).Tell(new CheckIfNotificationSubscriptionExistsMessage(SubscriptionId));  
            //});
         
            //ReceiveAsync<CheckIfNotificationSubscriptionExistsCompletedMessage>(async message =>
            //{
            //    if (!message.IsSubscribed)
            //    {
            //        Logger.Error("Notification subscription is no longer valid. Trying to subscribe again ....");
            //        await   SubscribeToRemoteNotificationActorMessasges(inventoryActorAddress);
            //    }
            //    else
            //    {
            //        Logger.Debug("Notification subscription is still valid");
            //    }
            //});



            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(1), Self, new SubscribeToNotifications(), Self);

            //Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(30), Self,new MonitorHealthMessage(), Self);

            //Receive<ActorAliveReceivedMessage>(message => SendKeepAlive(inventoryActorAddress));
            Receive<Terminated>( t => {
                Logger.Error("Suddenly unsubscribed from notification at " + t.ActorRef .Path+ ". trying to subscribe again ");
                Self.Tell(new SubscribeToNotifications());
            });
        }

        //private void SendKeepAlive(string inventoryActorAddress)
        //{
        //    Logger.Debug("Sending keep alive to " + inventoryActorAddress);
        //    Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(10), NotificationsActorRef, new ActorAliveMessage(SubscriptionId), Self);
        //}

        public IActorRef NotificationsActorRef { get; set; }

        private async Task<IActorRef> SubscribeToRemoteNotificationActorMessasges(string inventoryActorAddress)
        {
            var notificationsActor = Context.System.ActorSelection(inventoryActorAddress);
            Logger.Debug("Trying to reach remote actor at  " + inventoryActorAddress + " ....");
            var isReachable = false;
            var retryMax = 10;
            var expDelay = 0;
            IActorRef notificationsActorRef = null;
            while (!isReachable && retryMax > 0)
            {
                try
                {
                    notificationsActorRef = await notificationsActor.ResolveOne(TimeSpan.FromSeconds(3));
                 
                    isReachable = true;
                    Logger.Debug("Successfully reached " + inventoryActorAddress + " ....");
                }
                catch (Exception e)
                {
                    retryMax--;
                    await Task.Delay(TimeSpan.FromSeconds(expDelay++));
                    isReachable = false;
                    Logger.Error("remote actor is not reachable, so im retrying " + inventoryActorAddress + " ....", e);
                }
            }

            if (isReachable)
            {
                Logger.Debug("Subscribing for notifications at " + inventoryActorAddress + " ....");
                Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), notificationsActor, new SubScribeToNotificationMessage(), Self);
            }
            else
            {
                //kill the actor
                await Self.GracefulStop(TimeSpan.FromSeconds(10));
            }
            return await Task.FromResult(notificationsActorRef);
        }

        public class SubscribeToNotifications
        {
        }
    }

    public class MonitorHealthMessage
    {
    }
}