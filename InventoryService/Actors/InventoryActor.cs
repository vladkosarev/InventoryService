using System;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;
using InventoryService.Messages;
using InventoryService.Repository;
using InventoryService.Storage;

namespace InventoryService.Actors
{
    public class InventoryActor : ReceiveActor
    {
        private readonly Dictionary<string, IActorRef> _products = new Dictionary<string, IActorRef>();
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private int _reserveMessageCount = 0;
        private int _reserveMessageLastCount = 0;

        public InventoryActor(IInventoryStorage inventoryStorage)
        {
            Context.System.Scheduler.ScheduleTellRepeatedly(
                TimeSpan.Zero
                , TimeSpan.FromMilliseconds(1000)
                , Self
                , new GetMetrics()
                , ActorRefs.Nobody);

            Receive<ReserveMessage>(message =>
            {
                _reserveMessageCount++;

                if (!_products.ContainsKey(message.ProductId))
                {
                    var productActorRef = Context.ActorOf(
                        Props.Create(() =>
                            new ProductInventoryActor(inventoryStorage, message.ProductId))
                            , message.ProductId);
                    _products.Add(message.ProductId, productActorRef);
                }
                _products[message.ProductId].Forward(message);
            });

            Receive<PurchaseMessage>(message =>
                {
                    if (!_products.ContainsKey(message.ProductId))
                    {
                        var productActorRef = Context.ActorOf(
                            Props.Create(() =>
                                new ProductInventoryActor(inventoryStorage, message.ProductId))
                            , message.ProductId);
                        _products.Add(message.ProductId, productActorRef);
                    }
                    _products[message.ProductId].Forward(message);
                });

            Receive<GetMetrics>(message =>
            {
                _stopwatch.Stop();
                var perf = (_reserveMessageCount - _reserveMessageLastCount) / _stopwatch.Elapsed.TotalSeconds;
                _reserveMessageLastCount = _reserveMessageCount;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("\r{0} m/s                           ", (int)perf);
                _stopwatch.Restart();
            });
        }
    }
}

