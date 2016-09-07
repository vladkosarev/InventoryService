using System;
using Akka.Actor;
using Akka.Event;
using InventoryService.Messages;
using InventoryService.Messages.Models;
using InventoryService.Messages.Request;
using InventoryService.Messages.Response;
using InventoryService.Storage;
using System.Collections.Generic;
using System.Linq;
using InventoryService.NotificationActor;

namespace InventoryService.Actors
{
    public class InventoryActor : ReceiveActor
    {
        private readonly Dictionary<string, IActorRef> _products = new Dictionary<string, IActorRef>();
        private readonly Dictionary<string, RealTimeInventory> _realTimeInventories = new Dictionary<string, RealTimeInventory>();
        private readonly Dictionary<string, RemoveProductMessage> _removedRealTimeInventories = new Dictionary<string, RemoveProductMessage>();
        public readonly ILoggingAdapter Logger = Context.GetLogger();
        private readonly bool _withCache;
       // private bool HasUpdate { set; get; }

        public InventoryActor(IInventoryStorage inventoryStorage, IPerformanceService performanceService, bool withCache = true)
        {

             NotificationActorRef = Context.ActorOf(Props.Create(() =>new NotificationsActor()));

            _withCache = withCache;

            performanceService.Init();

            Receive<RemoveProductMessage>(message =>
            {
                var productId = message?.RealTimeInventory?.ProductId;
                if (!string.IsNullOrEmpty(productId))
                {
                    _products.Remove(productId);
                    _realTimeInventories.Remove(productId);
                    _removedRealTimeInventories[productId] = message;
                    NotificationActorRef.Tell(productId+" Actor has died! Reason : " + message.Reason.Message);
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
            Receive<GetMetricsMessage>(message =>
            {
                performanceService.PrintMetrics();
            });

            Receive<IRequestMessage>(message =>
            {
               //HasUpdate = true;
                var eventName = message.GetType().Name + "Count";
                performanceService.Increment(eventName);
                GetActorRef(inventoryStorage, message.ProductId).Forward(message);
                GetActorRef(inventoryStorage, message.ProductId).Tell(new GetInventoryMessage(message.ProductId));
                //todo Self.Tell(new GetMetricsMessage());
            });

            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0),TimeSpan.FromSeconds(5), Self, new QueryInventoryListMessage(), NotificationActorRef);
        }

        public IActorRef NotificationActorRef { get; set; }

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