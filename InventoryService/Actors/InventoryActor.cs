using System;
using System.Collections.Generic;
using System.Diagnostics;
using Akka.Actor;
using InventoryService.Messages;
using InventoryService.Storage;
using InventoryService.Storage;

namespace InventoryService.Actors
{
    public class InventoryActor : ReceiveActor
    {
        private readonly Dictionary<string, IActorRef> _products = new Dictionary<string, IActorRef>();
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private int _reserveMessageCount = 0;
        private int _reserveMessageLastCount = 0;

        private int _purchaseMessageCount = 0;
        private int _purchaseMessageLastCount = 0;

        private int _getMessageCount = 0;
        private int _getMessageLastCount = 0;

        public InventoryActor(IInventoryStorage inventoryStorage)
        {
            Context.System.Scheduler.ScheduleTellRepeatedly(
                TimeSpan.Zero
                , TimeSpan.FromMilliseconds(1000)
                , Self
                , new GetMetrics()
                , ActorRefs.Nobody);

            Receive<GetInventoryMessage>(message =>
            {
                _getMessageCount++;

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
                _purchaseMessageCount++;
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
                var reserved = (_reserveMessageCount - _reserveMessageLastCount) / _stopwatch.Elapsed.TotalSeconds;
                var get = (_getMessageCount - _getMessageLastCount) / _stopwatch.Elapsed.TotalSeconds;
                var puchase = (_purchaseMessageCount - _purchaseMessageLastCount) / _stopwatch.Elapsed.TotalSeconds;
                _reserveMessageLastCount = _reserveMessageCount;
                _getMessageLastCount = _getMessageCount;
                _purchaseMessageLastCount = _purchaseMessageCount;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.SetCursorPosition(0,0);
                Console.Write("\r\n{0} m/s {1} total                                                       ", (int)get, _getMessageCount);
                Console.Write("\r\n{0} m/s {1} total                                                       ", (int)reserved, _reserveMessageCount);
                Console.Write("\r\n{0} m/s {1} total                                                       ", (int)puchase, _purchaseMessageCount);
                _stopwatch.Restart();
            });
        }
    }
}

