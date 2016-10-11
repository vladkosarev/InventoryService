using Akka.Actor;
using Akka.Event;
using InventoryService.Actors.Messages;
using InventoryService.Messages;
using InventoryService.Messages.Models;
using InventoryService.Messages.Request;
using InventoryService.Messages.Response;
using InventoryService.NotificationActor;
using InventoryService.Storage;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InventoryService.Actors
{
    public class InventoryActor : ReceiveActor
    {
        private readonly Dictionary<string, IActorRef> _products = new Dictionary<string, IActorRef>();
        private readonly Dictionary<string, RealTimeInventory> _realTimeInventories = new Dictionary<string, RealTimeInventory>();
        private readonly Dictionary<string, RemoveProductMessage> _removedRealTimeInventories = new Dictionary<string, RemoveProductMessage>();
        public readonly ILoggingAdapter Logger = Context.GetLogger();
        private IInventoryStorage InventoryStorage { set; get; }
        private readonly bool _withCache;
        public IActorRef NotificationActorRef { get; set; }

        public InventoryActor(IInventoryStorage inventoryStorage, bool withCache = true)
        {
            InventoryStorage = inventoryStorage;
            _withCache = withCache;
            Become(Initializing);
        }

        private void Initializing()
        {
            Self.Tell(new InitializeInventoriesFromStorageMessage());
            ReceiveAsync<InitializeInventoriesFromStorageMessage>(async message =>
            {
                var inventoryIdsResult = await InventoryStorage.ReadAllInventoryIdAsync();

                if (inventoryIdsResult.IsSuccessful)
                {
                    Become(Processing);
                    foreach (var s in inventoryIdsResult.Result)
                    {
                        GetActorRef(InventoryStorage, s).Tell(new GetInventoryMessage(s));
                    }
                }
                else
                {
                    throw new Exception("Failed to read inventories from storage " + InventoryStorage.GetType().FullName, inventoryIdsResult.Errors.Flatten());

                    /*
                 TODO :
                     Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5), Self, new QueryInventoryListMessage(), NotificationActorRef);

                 */
                }
            });
        }

        private void Processing()
        {
            NotificationActorRef = Context.ActorOf(Props.Create(() => new NotificationsActor()));

            Receive<RemoveProductMessage>(message =>
            {
                var productId = message?.RealTimeInventory?.ProductId;
                if (!string.IsNullOrEmpty(productId))
                {
                    _products.Remove(productId);
                    _realTimeInventories.Remove(productId);
                    _removedRealTimeInventories[productId] = message;
                    NotificationActorRef.Tell(productId + " Actor has died! Reason : " + message.Reason.Message);
                }
            });

            Receive<GetRemovedProductMessage>(message =>
            {
                Sender.Tell(new GetRemovedProductCompletedMessage(_removedRealTimeInventories.Select(x => x.Value).ToList()));
            });

            Receive<QueryInventoryListMessage>(message =>
            {
                Sender.Tell(new QueryInventoryListCompletedMessage(_realTimeInventories.Select(x => x.Value).ToList()));
            });

            Receive<GetInventoryCompletedMessage>(message =>
            {
                _realTimeInventories[message.RealTimeInventory.ProductId] = message.RealTimeInventory as RealTimeInventory;
            });
          

            Receive<IRequestMessage>(message =>
            {
                message.Sender = Sender;
                Logger.Error(message.GetType().Name + " received for " + message.ProductId + " for update " + message.Update);
                GetActorRef(InventoryStorage, message.ProductId).Forward(message);
                GetActorRef(InventoryStorage, message.ProductId).Tell(new GetInventoryMessage(message.ProductId));
                NotificationActorRef.Tell(message);
            });

            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5), Self, new QueryInventoryListMessage(), NotificationActorRef);
        }

        private IActorRef GetActorRef(IInventoryStorage inventoryStorage, string productId)
        {
            if (_products.ContainsKey(productId)) return _products[productId];

            var productActorRef = Context.ActorOf(
                Props.Create(() =>
                    new ProductInventoryActor(inventoryStorage, productId, _withCache))
                , productId);

            _products.Add(productId, productActorRef);
            _realTimeInventories.Add(productId, new RealTimeInventory(productId, 0, 0, 0));
            return _products[productId];
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                x =>
                {
                    var message = x.Message + " - " + x.InnerException?.Message;
                    Logger.Error(message);
                    NotificationActorRef.Tell(message);
                    return Directive.Stop;
                });
        }
    }
}