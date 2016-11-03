using Akka.Actor;
using Akka.Event;
using InventoryService.Messages;
using InventoryService.Messages.NotificationSubscriptionMessages;
using InventoryService.Messages.Request;
using System;
using System.Collections.Generic;

namespace InventoryService.Actors
{
    public class NotificationsActor : ReceiveActor
    {
        private List<Tuple<Guid, IActorRef>> Subscribers { set; get; }
        public readonly ILoggingAdapter Logger = Context.GetLogger();
        private QueryInventoryListCompletedMessage LastReceivedInventoryListMessage { set; get; }
        private string LastReceivedServerMessage { set; get; }
        private int _messageCount = 0;

        public NotificationsActor()
        {
            Subscribers=new List<Tuple<Guid, IActorRef>>();
            LastReceivedServerMessage = "System started at " + DateTime.UtcNow;
            Logger.Debug(LastReceivedServerMessage);
            Receive<string>(message =>
            {
                LastReceivedServerMessage = string.IsNullOrEmpty(message) ? LastReceivedServerMessage : message;
                NotifySubscribers(new ServerNotificationMessage (LastReceivedServerMessage));
#if DEBUG
                Logger.Debug(LastReceivedServerMessage);
#endif
            });
            Receive<QueryInventoryListMessage>(message =>
            {
                NotifySubscribers(LastReceivedInventoryListMessage);
            });
            Receive<QueryInventoryListCompletedMessage>(message =>
            {
                LastReceivedInventoryListMessage = message;
                NotifySubscribers(message);
#if DEBUG
                Logger.Debug("total inventories in inventory service : " + message?.RealTimeInventories?.Count);
#endif
            });
            Receive<GetMetricsMessage>(message =>
            {
                NotifySubscribers(new GetMetricsCompletedMessage(_messageCount / Seconds));

                _messageCount = 0;
            });

            Receive<IRequestMessage>(message =>
            {
                _messageCount++;
                NotifySubscribers(message);
#if DEBUG
                Logger.Debug("received by inventory Actor - " + message.GetType().Name + " - " + message);
#endif
            });
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(Seconds), Self, new GetMetricsMessage(), Self);

            Receive<SubScribeToNotificationMessage>(message =>
            {
                //todo doing this coz im not sure to use sender or iactorref passed in message in remoting scenarios
                var subscriber = (message.Subscriber as IActorRef) ?? (Sender);
                if (subscriber==null || Sender.IsNobody())
                {
                    Logger.Error("No subscriber specified or subscriber is nobody");
                    Sender.Tell(new SubScribeToNotificationFailedMessage("No subscriber specified or subscriber is nobody"));
                }
                else
                {
                    var id = Guid.NewGuid();
                    Subscribers.Add(new Tuple<Guid, IActorRef>(id, subscriber));
                    Sender.Tell(new SubScribeToNotificationCompletedMessage(id));
                    Logger.Debug("Successfully subscribed . Subscription id : "+ id+" and subscriber is : "+subscriber.Path);
                }
            });
            Receive<UnSubScribeToNotificationMessage>(message =>
            {
                if (!Subscribers.Exists(x => x.Item1 == message.SubscriptionId))
                {
                    Logger.Error("Unable to unscubscribe : No subscriber specified or subscriber is nobody");
                    Sender.Tell(new SubScribeToNotificationFailedMessage("Subscription doesn't exists"));
                }
                else
                {
                    Logger.Debug("Unsubscribing  subscriber " + message.SubscriptionId+" .... ");
                    Subscribers.Remove(Subscribers.Find(x => x.Item1 == message.SubscriptionId));
                    Sender.Tell(new UnSubScribeToNotificationCompletedMessage());
                }
            });
        }

        private void NotifySubscribers<T>(T message)
        {
            foreach (var subscriber in Subscribers)
            {
                Logger.Debug("Sending " + typeof(T).Name + " to subscriber : " + subscriber.Item1);
                subscriber.Item2.Tell(message);
            }
        }

        public int Seconds = 1;
    }
}