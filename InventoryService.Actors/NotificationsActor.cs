using Akka.Actor;
using Akka.Event;
using InventoryService.Messages;
using InventoryService.Messages.Models;
using InventoryService.Messages.NotificationSubscriptionMessages;
using InventoryService.Messages.Request;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InventoryService.Actors
{
    public class NotificationsActor : ReceiveActor
    {
        private List<Tuple<Guid, IActorRef>> Subscribers { set; get; }
        public readonly ILoggingAdapter Logger = Context.GetLogger();
        private QueryInventoryListCompletedMessage LastReceivedInventoryListMessage { set; get; }

        private QueryInventoryListCompletedMessage ChangeSetOfLastReceivedInventoryListMessage { set; get; }

        public QueryInventoryListCompletedMessage CalculateInventoryListChanges(
            QueryInventoryListCompletedMessage oldList, QueryInventoryListCompletedMessage newList)
        {
            oldList = oldList ?? new QueryInventoryListCompletedMessage(new List<RealTimeInventory>());
            newList = newList ?? new QueryInventoryListCompletedMessage(new List<RealTimeInventory>());

            var result = new QueryInventoryListCompletedMessage(new List<RealTimeInventory>());
            foreach (var newItem in newList.RealTimeInventories)
            {
                foreach (var oldItem in oldList.RealTimeInventories
                    .Where(oldItem =>
                    (oldItem.ProductId != newItem.ProductId) &&
                    (oldItem.Quantity != newItem.Quantity ||
                     oldItem.Reserved != newItem.Reserved ||
                     oldItem.Holds != newItem.Holds)))
                {
                    result.RealTimeInventories.Add(newItem);
                }
            }

            return result;
        }

        private string LastReceivedServerMessage { set; get; }
        private int _messageCount = 0;

        public NotificationsActor()
        {
            Subscribers = new List<Tuple<Guid, IActorRef>>();
            LastReceivedServerMessage = "System started at " + DateTime.UtcNow;
            Logger.Debug(LastReceivedServerMessage);
            Receive<string>(message =>
            {
                LastReceivedServerMessage = string.IsNullOrEmpty(message) ? LastReceivedServerMessage : message;
                NotifySubscribersAndRemoveStaleSubscribers(new ServerNotificationMessage(LastReceivedServerMessage));
                Logger.Debug(LastReceivedServerMessage);
            });
            Receive<QueryInventoryListMessage>(message =>
            {
                NotifySubscribersAndRemoveStaleSubscribers(LastReceivedInventoryListMessage);
            });
            Receive<QueryInventoryListCompletedMessage>(message =>
            {
                ChangeSetOfLastReceivedInventoryListMessage = CalculateInventoryListChanges(LastReceivedInventoryListMessage, message);
                LastReceivedInventoryListMessage = message;
                NotifySubscribersAndRemoveStaleSubscribers(ChangeSetOfLastReceivedInventoryListMessage);
                Logger.Debug("total inventories in inventory service : " + message?.RealTimeInventories?.Count);
            });
            Receive<GetMetricsMessage>(message =>
            {
                NotifySubscribersAndRemoveStaleSubscribers(new GetMetricsCompletedMessage((double)_messageCount / Seconds));
                NotifySubscribersAndRemoveStaleSubscribers(new ServerNotificationMessage(LastReceivedServerMessage));
                NotifySubscribersAndRemoveStaleSubscribers(LastReceivedInventoryListMessage);
                _messageCount = 0;
            });

            Receive<GetAllInventoryListMessage>(message =>
            {
                NotifySubscribersAndRemoveStaleSubscribers(LastReceivedInventoryListMessage);
            });

            Receive<IRequestMessage>(message =>
            {
                _messageCount++;
                NotifySubscribersAndRemoveStaleSubscribers(message);
                Logger.Debug("received by inventory Actor - " + message.GetType().Name + " - " + message);
            });
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(Seconds), Self, new GetMetricsMessage(), Self);

            Receive<SubScribeToNotificationMessage>(message =>
            {
                //todo doing this coz im not sure to use sender or iactorref passed in message in remoting scenarios
                var subscriber = (message.Subscriber as IActorRef) ?? (Sender);
                if (subscriber == null || Sender.IsNobody())
                {
                    Logger.Error("No subscriber specified or subscriber is nobody");
                    Sender.Tell(new SubScribeToNotificationFailedMessage("No subscriber specified or subscriber is nobody"));
                }
                else
                {
                    var id = Guid.NewGuid();
                    Subscribers.Add(new Tuple<Guid, IActorRef>(id, subscriber));
                    Sender.Tell(new SubScribeToNotificationCompletedMessage(id));
                    Logger.Debug("Successfully subscribed . Subscription id : " + id + " and subscriber is : " + subscriber.Path);
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
                    Logger.Debug("Unsubscribing  subscriber " + message.SubscriptionId + " .... ");
                    Subscribers.Remove(Subscribers.Find(x => x.Item1 == message.SubscriptionId));
                    Sender.Tell(new UnSubScribeToNotificationCompletedMessage());
                }
                Subscribers.Remove(Subscribers.Find(x => string.IsNullOrEmpty(x?.Item1.ToString())));
            });
            Receive<CheckIfNotificationSubscriptionExistsMessage>(message =>
            {
                var subscriptionExists = Subscribers.Exists(x => x.Item1 == message.SubscriptionId);
                Sender.Tell(new CheckIfNotificationSubscriptionExistsCompletedMessage(subscriptionExists));
            });
        }

        private void NotifySubscribersAndRemoveStaleSubscribers<T>(T message)
        {
            foreach (var subscriber in Subscribers)
            {
                Logger.Debug("Sending " + typeof(T).Name + " to subscriber : " + subscriber?.Item1);
                if (
                    subscriber == null
                    || subscriber.Item2.IsNobody()
                    || subscriber.Item2 == null
                    || string.IsNullOrEmpty(subscriber.Item1.ToString()))
                {
                    if (subscriber != null)
                    {
                        Self.Tell(new UnSubScribeToNotificationMessage(subscriber.Item1));
                    }
                }
                else
                {
                    subscriber.Item2.Tell(message);
                }
            }
        }

        public int Seconds = 2;
    }
}