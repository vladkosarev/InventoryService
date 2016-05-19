using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private int _reserveMessageCount = 0;
        private int _reserveMessageLastCount = 0;

        private int _purchaseMessageCount = 0;
        private int _purchaseMessageLastCount = 0;

        private int _getMessageCount = 0;
        private int _getMessageLastCount = 0;

        public InventoryActor(IInventoryStorage inventoryStorage, bool withCache = true)
        {
            _withCache = withCache;

            Console.Clear();
            Console.CursorVisible = false;

            Context.System.Scheduler.ScheduleTellRepeatedly(
                TimeSpan.Zero
                , TimeSpan.FromMilliseconds(1000)
                , Self
                , new GetMetrics()
                , ActorRefs.Nobody);

            Receive<GetInventoryMessage>(message =>
            {
                _getMessageCount++;
                GetActorRef(inventoryStorage, message.ProductId);
            });

            Receive<ReserveMessage>(message =>
            {
                _reserveMessageCount++;
                GetActorRef(inventoryStorage, message.ProductId);
            });

            Receive<PurchaseMessage>(message =>
            {
                _purchaseMessageCount++;
                GetActorRef(inventoryStorage, message.ProductId).Forward(message);
            });

            Receive<GetMetrics>(message =>
            {
                PrintMetrics();
            });
        }

        private void PrintMetrics()
        {
            _stopwatch.Stop();
            var reserved = (_reserveMessageCount - _reserveMessageLastCount)/_stopwatch.Elapsed.TotalSeconds;
            var get = (_getMessageCount - _getMessageLastCount)/_stopwatch.Elapsed.TotalSeconds;
            var puchase = (_purchaseMessageCount - _purchaseMessageLastCount)/_stopwatch.Elapsed.TotalSeconds;
            _reserveMessageLastCount = _reserveMessageCount;
            _getMessageLastCount = _getMessageCount;
            _purchaseMessageLastCount = _purchaseMessageCount;
            var width = Console.WindowWidth;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(0, 0);
            Console.Write($"\r\n{(int) get} m/s {_getMessageCount} total reads".PadRight(width));
            Console.Write($"\r\n{(int) reserved} m/s {_reserveMessageCount} total reservations".PadRight(width));
            Console.Write($"\r\n{(int) puchase} m/s {_purchaseMessageCount} total purchases".PadRight(width));
            _stopwatch.Restart();
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

