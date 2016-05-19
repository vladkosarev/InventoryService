using System;
using Akka.Actor;
using InventoryService.Messages;
using InventoryService.Storage;

namespace InventoryService.Actors
{
    public class ProductInventoryActor : ReceiveActor
    {
        private readonly string _id;
        private int _quantity;
        private int _reservedQuantity;
        private bool _withCache;

        private readonly IInventoryStorage _inventoryStorage;

        public ProductInventoryActor(IInventoryStorage inventoryStorage, string id, bool withCache)
        {
            _id = id;
            _inventoryStorage = inventoryStorage;
            _withCache = withCache;

            var inventory = _inventoryStorage.ReadInventory(id).Result;
            _quantity = inventory.Item1;
            _reservedQuantity = inventory.Item2;

            Become(Running);

            //Context.System.Scheduler.ScheduleTellRepeatedly(
            //    TimeSpan.Zero
            //    , TimeSpan.FromMilliseconds(100)
            //    , Self
            //    , new FlushStreamMessage(_id)
            //    , ActorRefs.Nobody);
        }

        private void Running()
        {
            ReceiveAsync<GetInventoryMessage>(async message =>
            {
                if (!_withCache)
                {
                    var inventory = await _inventoryStorage.ReadInventory(message.ProductId);
                    _quantity = inventory.Item1;
                    _reservedQuantity = inventory.Item2;
                }
                Sender.Tell(new RetrievedInventoryMessage(message.ProductId, _quantity, _reservedQuantity));
            });

            ReceiveAsync<ReserveMessage>(async message =>
            {
                var newReservedQuantity = _reservedQuantity + message.ReservationQuantity;
                if (newReservedQuantity <= _quantity)
                {
                    var result = await _inventoryStorage.WriteInventory(
                        message.ProductId
                        , _quantity
                        , newReservedQuantity);

                    if (result)
                    {
                        _reservedQuantity = newReservedQuantity;
                        Sender.Tell(new ReservedMessage(_id, message.ReservationQuantity, true));
                    }
                    else
                    {
                        Sender.Tell(new ReservedMessage(_id, message.ReservationQuantity, false));
                    }
                }
                else
                {
                    Sender.Tell(new ReservedMessage(_id, message.ReservationQuantity, false));
                }
            });

            ReceiveAsync<PurchaseMessage>(async message =>
            {
                var newQuantity = _quantity - message.Quantity;
                var newReservedQuantity = Math.Max(0,_reservedQuantity - message.Quantity);

                if (newQuantity >= 0)
                {
                    var result = await _inventoryStorage.WriteInventory(
                        message.ProductId
                        , newQuantity
                        , newReservedQuantity);

                    if (result)
                    {
                        _quantity = newQuantity;
                        _reservedQuantity = newReservedQuantity;
                        Sender.Tell(new PurchasedMessage(_id, message.Quantity, true));
                    }
                    else
                    {
                        Sender.Tell(new PurchasedMessage(_id, message.Quantity, false));
                    }
                }
                else
                {
                    Sender.Tell(new PurchasedMessage(_id, message.Quantity, false));
                }
            });

            Receive<FlushStreamMessage>(message =>
            {
                _inventoryStorage.Flush(_id);
            });
        }
    }
}

