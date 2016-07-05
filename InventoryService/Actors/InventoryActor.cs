using System;
using System.Collections.Generic;
using Akka.Actor;
using InventoryService.Messages;
using InventoryService.Storage;

namespace InventoryService.Actors
{
    public class InventoryActor : ReceiveActor
    {
        private readonly Dictionary<string, IActorRef> _products =
            new Dictionary<string, IActorRef>();

        private readonly bool _withCache;

        public InventoryActor(IInventoryStorage inventoryStorage, IPerformanceService performanceService, bool withCache = true)
        {
            _withCache = withCache;

            performanceService.Init();

            Context.System.Scheduler.ScheduleTellRepeatedly(
                TimeSpan.Zero
                , TimeSpan.FromMilliseconds(1000)
                , Self
                , new GetMetrics()
                , ActorRefs.Nobody);

            Receive<GetInventoryMessage>(message =>
            {
                performanceService.Increment("getMessageCount");
                GetActorRef(inventoryStorage, message.ProductId).Forward(message);
            });

            Receive<ReserveMessage>(message =>
            {
                performanceService.Increment("reserveMessageCount");
                GetActorRef(inventoryStorage, message.ProductId).Forward(message);
            });

            Receive<PurchaseMessage>(message =>
            {
                performanceService.Increment("purchaseMessageCount");
                GetActorRef(inventoryStorage, message.ProductId).Forward(message);
            });

            Receive<GetMetrics>(message =>
            {
                performanceService.PrintMetrics();
            });
        }        

        private IActorRef GetActorRef(IInventoryStorage inventoryStorage, string productId)
        {
            if (!_products.ContainsKey(productId))
            {
                var productActorRef = Context.ActorOf(
                    Props.Create(() =>
                        new ProductInventoryActor(inventoryStorage, productId, _withCache))
                        , productId);
                _products.Add(productId, productActorRef);
            }

            return _products[productId];
        }
    }
}

