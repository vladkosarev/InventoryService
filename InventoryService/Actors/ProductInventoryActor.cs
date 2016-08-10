using System;
using System.Threading.Tasks;
using Akka.Actor;
using InventoryService.Messages;
using InventoryService.Storage;

namespace InventoryService.Actors
{
    public class ProductInventoryActor : ReceiveActor
    {
        private readonly string _id;
        private int _quantity;
        private int _reservations;
        private int _holds;

        private readonly bool _withCache;

        private readonly IInventoryStorage _inventoryStorage;

        public ProductInventoryActor(IInventoryStorage inventoryStorage, string id, bool withCache)
        {
            _id = id;
            _inventoryStorage = inventoryStorage;
            _withCache = withCache;

            var inventory = _inventoryStorage.ReadInventory(id).Result;
            _quantity = inventory.Item1;
            _reservations = inventory.Item2;
            _holds = inventory.Item3;

            Become(Running);

            //Context.System.Scheduler.ScheduleTellRepeatedly(
            //    TimeSpan.Zero
            //    , TimeSpan.FromMilliseconds(100)
            //    , Self
            //    , new FlushStreamMessage(_id)
            //    , ActorRefs.Nobody);
        }

        private async Task<bool> Reserve(string productId, int reservationQuantity)
        {
            var newReserved = _reservations + reservationQuantity;
            if (newReserved > _quantity - _holds) return false;
            var result = await _inventoryStorage.WriteInventory(
                productId
                , _quantity
                , newReserved
                , _holds);

            if (!result) return false;
            _reservations = newReserved;
            return true;
        }

        private async Task<bool> Purchase(string productId, int quantity)
        {
            var newQuantity = _quantity - quantity;
            var newReserved = Math.Max(0, _reservations - quantity);

            if (newQuantity - _holds < 0) return false;
            var result = await _inventoryStorage.WriteInventory(
                productId
                , newQuantity
                , newReserved
                , _holds);

            if (!result) return false;
            _quantity = newQuantity;
            _reservations = newReserved;
            return true;
        }

        private void Running()
        {
            ReceiveAsync<GetInventoryMessage>(async message =>
            {
                if (!_withCache)
                {
                    var inventory = await _inventoryStorage.ReadInventory(message.ProductId);
                    _quantity = inventory.Item1;
                    _reservations = inventory.Item2;
                    _holds = inventory.Item3;
                }
                Sender.Tell(new RetrievedInventoryMessage(message.ProductId, _quantity, _reservations));
            });

            ReceiveAsync<ReserveMessage>(async message =>
            {
                Sender.Tell(
                    new ReservedMessage(
                        message.ProductId
                        , message.ReservationQuantity
                        , await Reserve(message.ProductId, message.ReservationQuantity)));
            });

            ReceiveAsync<PurchaseMessage>(async message =>
            {
                Sender.Tell(
                    new PurchasedMessage(
                        message.ProductId
                        , message.Quantity
                        , await Purchase(message.ProductId, message.Quantity)));
            });

            Receive<FlushStreamsMessage>(message => { _inventoryStorage.Flush(_id); });
        }
    }
}