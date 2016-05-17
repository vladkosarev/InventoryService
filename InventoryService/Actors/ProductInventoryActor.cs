using System;
using System.Threading.Tasks;
using Akka.Actor;
using InventoryService.Messages;
using InventoryService.Repository;
using InventoryService.Storage;

namespace InventoryService.Actors
{
    public class ProductInventoryActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly string _id;
        private int _quantity;
        private int _reservedQuantity;
        private IInventoryStorage _inventoryStorage;
        public IStash Stash { get; set; }

        public ProductInventoryActor(IInventoryStorage inventoryStorage, string id)
        {
            _id = id;
            _inventoryStorage = inventoryStorage;
            Become(Running);
            var inventory = _inventoryStorage.ReadInventory(id).Result;
            _quantity = inventory.Item1;
            _reservedQuantity = inventory.Item2;
            //Context.System.Scheduler.ScheduleTellRepeatedly(
            //    TimeSpan.Zero
            //    , TimeSpan.FromMilliseconds(100)
            //    , Self
            //    , new FlushStreamMessage(_id)
            //    , ActorRefs.Nobody);
        }

        private void Running()
        {
            ReceiveAsync<ReserveMessage>(async message =>
            {
                var newReservedQuantity = _reservedQuantity + message.ReservationQuantity;
                if (newReservedQuantity <= _quantity)
                {
                    //if (message.ProductId == "product0") Console.WriteLine("{0} - {1} [{2}]", message.ProductId, _reservedQuantity, Sender.Path);
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

            Receive<PurchaseMessage>(message =>
            {
                var newQuantity = _quantity - message.Quantity;
                if (newQuantity >= 0)
                {
                    // write to repository here
                    _quantity = newQuantity;
                    Sender.Tell(new PurchasedMessage(_id, message.Quantity, true));
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

