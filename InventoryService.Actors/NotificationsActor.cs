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
        protected List<Tuple<string, IActorRef>> Subscribers { set; get; }
        protected HashSet<string> ActorAliveList { set; get; }

        protected Dictionary<string, IRealTimeInventory> RealTimeInventories { set; get; }

        protected string LastReceivedServerMessage { set; get; }
        protected double MessageCount = 0;
        protected double MessageSpeed = 0;
        protected double PeakMessageSpeed = 0;
        public double MessageSampleRate = 1;

        public NotificationsActor()
        {
            var lastUpadteTime = DateTime.UtcNow;
            Subscribers = new List<Tuple<string, IActorRef>>();
            LastReceivedServerMessage = "System started at " + DateTime.UtcNow;
            RealTimeInventories = new Dictionary<string, IRealTimeInventory>();
            Logger.Debug(LastReceivedServerMessage);
            Receive<string>(message =>
            {
                LastReceivedServerMessage = string.IsNullOrEmpty(message) ? LastReceivedServerMessage : message;
                //NotifySubscribersAndRemoveStaleSubscribers(new ServerNotificationMessage(LastReceivedServerMessage));
                Logger.Debug(LastReceivedServerMessage);
            });
            Receive<QueryInventoryListMessage>(message =>
            {
                NotifySubscribersAndRemoveStaleSubscribers(RealTimeInventories);
            });
            Receive<RealTimeInventoryChangeMessage>(message =>
            {
                RealTimeInventories[message.RealTimeInventory.ProductId] = message.RealTimeInventory;
                NotifySubscribersAndRemoveStaleSubscribers(message);
                Logger.Debug("total inventories in inventory service : " +RealTimeInventories.Count);
            });
           

            Receive<GetAllInventoryListMessage>(message =>
            {
               Sender.Tell(new QueryInventoryCompletedMessage(RealTimeInventories.Select(x => x.Value).ToList(), 0, 0));
            });

            Receive<IRequestMessage>(message =>
            {
                NotifySubscribersAndRemoveStaleSubscribers(message);

                Logger.Debug("received by inventory Actor - " + message.GetType().Name + " - " + message);

                MessageCount++;
                var secondsPast = (int)(DateTime.UtcNow - lastUpadteTime).TotalSeconds;

                if (secondsPast <= 1) return;

                MessageSpeed = MessageCount / secondsPast;
                PeakMessageSpeed = (PeakMessageSpeed > MessageSpeed) ? PeakMessageSpeed : MessageSpeed;
                lastUpadteTime = DateTime.UtcNow;
                MessageCount = 0;
            });
        
            /*
             TODO REMOVING THESE COZ USING INFRASTR..
                      Receive<ActorAliveMessage>(message =>
                     {
                         Logger.Debug(message.SubscriptionId + " from " + Sender.Path + " says it is still alive");
                         ActorAliveList = ActorAliveList ?? new HashSet<string>();
                         var sub = Subscribers.FirstOrDefault(x => x.Item1 == message.SubscriptionId);
                         if (sub != null)
                         {
                             ActorAliveList.Add(message.SubscriptionId);
                             sub.Item2.Tell(new ActorAliveReceivedMessage());
                         }
                     });

                     Receive<PurgeInvalidSubscribers>(message =>
                     {
                         ActorAliveList = ActorAliveList ?? new HashSet<string>();
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
                         ActorAliveList = new HashSet<string>();
                     });

                     Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(20), Self, new PurgeInvalidSubscribers(), Self);
                      */
            Receive<Terminated>(t => {
                Logger.Error("Removing subscriber " + t.ActorRef.Path.ToStringWithUid() + " because no ActorAliveMessage was received over time ....");
                Self.Tell(new UnSubScribeToNotificationMessage(t.ActorRef.Path.ToStringWithUid()));
            });


            Receive<SubScribeToNotificationMessage>(message =>
            {
              
                if (Sender.IsNobody())
                {
                    Logger.Error("No subscriber specified or subscriber is nobody");
                }
                else
                {
                    var id = Sender.Path.ToStringWithUid();
                    Subscribers.Add(new Tuple<string, IActorRef>(id, Sender));
                    ActorAliveList = ActorAliveList ?? new HashSet<string>();
                    ActorAliveList.Add(id);
                    Context.Watch(Sender);
                    Sender.Tell(new SubScribeToNotificationCompletedMessage(id));
                    Logger.Debug("Successfully subscribed . Subscription id : " + id + " and subscriber is : " + Sender.Path);
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

       
    }

    public class PurgeInvalidSubscribers
    {
    }
}