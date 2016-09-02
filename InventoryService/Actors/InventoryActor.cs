using Akka.Actor;
using InventoryService.Messages;
using InventoryService.Messages.Request;
using InventoryService.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using InventoryService.Messages.Models;
using InventoryService.Messages.Response;

namespace InventoryService.Actors
{
    public class InventoryActor : ReceiveActor
    {
        private readonly Dictionary<string, IActorRef> _products = new Dictionary<string, IActorRef>();
        private readonly Dictionary<string, RealTimeInventory> _realTimeInventories = new Dictionary<string, RealTimeInventory>();

        private readonly bool _withCache;

        public InventoryActor(AnInventoryStorage inventoryStorage, IPerformanceService performanceService, bool withCache = true)
        {
            _withCache = withCache;

            performanceService.Init();

            //Context.System.Scheduler.ScheduleTellRepeatedly(
            //    TimeSpan.Zero
            //    , TimeSpan.FromMilliseconds(1000)
            //    , Self
            //    , new GetMetricsMessage()
            //    , ActorRefs.Nobody);


            Receive<QueryInventoryListMessage>(message =>
            {
                Sender.Tell(  new QueryInventoryListCompletedMessage(_realTimeInventories.Select(x => x.Value).ToList()));   
                foreach (var product in _products)
                {
                    GetActorRef(inventoryStorage, product.Key).Tell(new GetInventoryMessage(product.Key));
                }
            });
            Receive<GetInventoryCompletedMessage>(message =>
            {
                _realTimeInventories[message.RealTimeInventory.ProductId]=message.RealTimeInventory as RealTimeInventory;
            });
            Receive<GetMetricsMessage>(message =>
            {
                performanceService.PrintMetrics();
            });

            Receive<IRequestMessage>(message =>
            {
                var eventName = message.GetType().Name + "Count";
                performanceService.Increment(eventName);
                GetActorRef(inventoryStorage, message.ProductId).Forward(message);
                Self.Tell(new GetMetricsMessage());
            });
        }

        private IActorRef GetActorRef(AnInventoryStorage inventoryStorage, string productId)
        {
            if (_products.ContainsKey(productId)) return _products[productId];

            var productActorRef = Context.ActorOf(
                Props.Create(() =>
                    new ProductInventoryActor(inventoryStorage, productId, _withCache))
                , productId);

            _products.Add(productId, productActorRef);
            _realTimeInventories.Add(productId,new RealTimeInventory(productId, 0,0,0));
            return _products[productId];
        }
    }
}