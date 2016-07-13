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
        private int _reservedQuantity;
        private readonly bool _withCache;

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

        private async Task<bool> Reserve(string productId, int reservationQuantity)
        {
            var newReservedQuantity = _reservedQuantity + reservationQuantity;
            if (newReservedQuantity > _quantity) return false;
            var result = await _inventoryStorage.WriteInventory(
                productId
                , _quantity
                , newReservedQuantity);

            if (!result) return false;
            _reservedQuantity = newReservedQuantity;
            return true;
        }

        private async Task<bool> Purchase(string productId, int quantity)
        {
            var newQuantity = _quantity - quantity;
            var newReservedQuantity = Math.Max(0, _reservedQuantity - quantity);

            if (newQuantity < 0) return false;
            var result = await _inventoryStorage.WriteInventory(
                productId
                , newQuantity
                , newReservedQuantity);

            if (!result) return false;
            _quantity = newQuantity;
            _reservedQuantity = newReservedQuantity;
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
                    _reservedQuantity = inventory.Item2;
                }
                Sender.Tell(new RetrievedInventoryMessage(message.ProductId, _quantity, _reservedQuantity));
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