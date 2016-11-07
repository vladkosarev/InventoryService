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
        public readonly ILoggingAdapter Logger = Context.GetLogger();
        protected List<Tuple<Guid, IActorRef>> Subscribers { set; get; }
        protected HashSet<Guid> ActorAliveList { set; get; }

        protected QueryInventoryListCompletedMessage LastReceivedInventoryListMessage { set; get; }

        public QueryInventoryListCompletedMessage CalculateInventoryListChangesAndUpdateCurrentTotal(
            QueryInventoryListCompletedMessage oldList, QueryInventoryListCompletedMessage newList)
        {
            oldList = oldList ?? new QueryInventoryListCompletedMessage(new List<IRealTimeInventory>());
            newList = newList ?? new QueryInventoryListCompletedMessage(new List<IRealTimeInventory>());

            var result = new QueryInventoryListCompletedMessage(new List<IRealTimeInventory>());
            foreach (var newItem in newList.RealTimeInventories)
            {
                if (!oldList.RealTimeInventories.Exists(x => x.ProductId == newItem.ProductId))
                {
                    result.RealTimeInventories.Add(newItem);
                    oldList.RealTimeInventories.Add(newItem);
                }
                else
                {
                    for (var i = 0; i < oldList.RealTimeInventories.Count; i++)
                    {
                        var oldItem = oldList.RealTimeInventories[i];
                        if ((oldItem.ProductId != newItem.ProductId) || (oldItem.Quantity == newItem.Quantity && oldItem.Reserved == newItem.Reserved && oldItem.Holds == newItem.Holds)) continue;
                        result.RealTimeInventories.Add(newItem);
                        oldList.RealTimeInventories[i] = newItem;
                    }
                }

            }

            return result;
        }

        protected string LastReceivedServerMessage { set; get; }
        protected double MessageCount = 0;

        public NotificationsActor()
        {

            Subscribers = new List<Tuple<Guid, IActorRef>>();
            LastReceivedServerMessage = "System started at " + DateTime.UtcNow;
            LastReceivedInventoryListMessage = new QueryInventoryListCompletedMessage(new List<IRealTimeInventory>());
            Logger.Debug(LastReceivedServerMessage);
            Receive<string>(message =>
            {
                LastReceivedServerMessage = string.IsNullOrEmpty(message) ? LastReceivedServerMessage : message;
                //NotifySubscribersAndRemoveStaleSubscribers(new ServerNotificationMessage(LastReceivedServerMessage));
                Logger.Debug(LastReceivedServerMessage);
            });
            Receive<QueryInventoryListMessage>(message =>
            {
                NotifySubscribersAndRemoveStaleSubscribers(LastReceivedInventoryListMessage);
            });
            Receive<QueryInventoryListCompletedMessage>(message =>
            {
                var changeSetOfLastReceivedInventoryListMessage = CalculateInventoryListChangesAndUpdateCurrentTotal(LastReceivedInventoryListMessage, message);
                
                if (changeSetOfLastReceivedInventoryListMessage.RealTimeInventories != null &&
                    changeSetOfLastReceivedInventoryListMessage.RealTimeInventories.Count > 0)
                {
                    NotifySubscribersAndRemoveStaleSubscribers(changeSetOfLastReceivedInventoryListMessage);
                }

                Logger.Debug("total inventories in inventory service : " + message?.RealTimeInventories?.Count);
            });
            Receive<GetMetricsMessage>(message =>
            {

                if (MessageCount > 2)
                {
                    throw  new Exception();
                }

                NotifySubscribersAndRemoveStaleSubscribers(new GetMetricsCompletedMessage(MessageCount / Seconds));

                MessageCount = 0;
            });

            Receive<GetAllInventoryListMessage>(message =>
            {
                NotifySubscribersAndRemoveStaleSubscribers(LastReceivedInventoryListMessage);
            });

            Receive<IRequestMessage>(message =>
            {
                MessageCount++;
                NotifySubscribersAndRemoveStaleSubscribers(message);
                Logger.Debug("received by inventory Actor - " + message.GetType().Name + " - " + message);
            });
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(Seconds), Self, new GetMetricsMessage(), Self);

            Receive<ActorAliveMessage>(message =>
            {
                Logger.Debug(message.SubscriptionId + " from " + Sender.Path + " says it is still alive");
                ActorAliveList = ActorAliveList ?? new HashSet<Guid>();
                var sub = Subscribers.FirstOrDefault(x => x.Item1 == message.SubscriptionId);
                if (sub != null)
                {
                    ActorAliveList.Add(message.SubscriptionId);
                    sub.Item2.Tell(new ActorAliveReceivedMessage());
                }
            });

            Receive<PurgeInvalidSubscribers>(message =>
            {
                ActorAliveList = ActorAliveList ?? new HashSet<Guid>();
                foreach (var subscriber in Subscribers)
                {
                    if (!ActorAliveList.Contains(subscriber.Item1))
                    {
                        Logger.Error("Removing subscriber " + subscriber.Item1 + " because no ActorAliveMessage was received over time ....");
                        Self.Tell(new UnSubScribeToNotificationMessage(subscriber.Item1));
                        subscriber.Item2.Tell(new UnSubscribedNotificationMessage(subscriber.Item1));
                    }
                    else
                    {
                        Logger.Debug("Subscriber " + subscriber.Item1 + " is still connected!!");
                    }
                }
                ActorAliveList = new HashSet<Guid>();
            });

            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(20), Self, new PurgeInvalidSubscribers(), Self);

            Receive<SubScribeToNotificationMessage>(message =>
            {
                var subscriber = Sender;
                if (Sender.IsNobody())
                {
                    Logger.Error("No subscriber specified or subscriber is nobody");
                }
                else
                {
                    var id = Guid.NewGuid();
                    Subscribers.Add(new Tuple<Guid, IActorRef>(id, subscriber));
                    ActorAliveList.Add(id);
                    Sender.Tell(new SubScribeToNotificationCompletedMessage(id));
                    Logger.Debug("Successfully subscribed . Subscription id : " + id + " and subscriber is : " + subscriber.Path);
                }
            });
            Receive<UnSubScribeToNotificationMessage>(message =>
            {
                if (!Subscribers.Exists(x => x.Item1 == message.SubscriptionId))
                {
                    Logger.Error("Unable to unsubscribe : No subscriber specified or subscriber is nobody");
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

        protected void NotifySubscribersAndRemoveStaleSubscribers<T>(T message)
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

    public class PurgeInvalidSubscribers
    {
    }
}