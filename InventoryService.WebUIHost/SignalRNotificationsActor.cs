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

            ReceiveAsync<SubscribeToNotifications>(async _ => await SubscribeToRemoteNotificationActorMessasges(inventoryActorAddress));

            Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(1), Self, new SubscribeToNotifications(), Self);
        }

        private async Task<bool> SubscribeToRemoteNotificationActorMessasges(string inventoryActorAddress)
        {
            var notificationsActor = Context.System.ActorSelection(inventoryActorAddress);
            Logger.Debug("Trying to reach remote actor at  " + inventoryActorAddress + " ....");
            var isReachable = false;
            var retryMax = 10;
            var expDelay = 0;
            while (!isReachable && retryMax > 0)
            {
                try
                {
                    await notificationsActor.ResolveOne(TimeSpan.FromSeconds(3));
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
                Context.System.Scheduler.ScheduleTellOnce(TimeSpan.FromSeconds(3), notificationsActor,
                    new SubScribeToNotificationMessage(Self), Self);
                return await Task.FromResult(true);
            }
            else
            {
                //kill the actor
                await Self.GracefulStop(TimeSpan.FromSeconds(10));
                return await Task.FromResult(false);
            }
        }

        public class SubscribeToNotifications
        {
        }
    }
}