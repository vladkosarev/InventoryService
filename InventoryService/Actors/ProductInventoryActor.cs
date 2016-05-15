using System;
using System.Threading.Tasks;
using Akka.Actor;
using InventoryService.Messages;
using InventoryService.Repository;

namespace InventoryService.Actors
{
    public class ProductInventoryActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly string _id;
        private int _quantity;
        private int _reservedQuantity;
        private IInventoryServiceRepository _inventoryServiceRepository;
        public IStash Stash { get; set; }

        public ProductInventoryActor(IInventoryServiceRepository inventoryServiceRepository, string id)
        {
            _id = id;
            _inventoryServiceRepository = inventoryServiceRepository;
            Become(Running);
            var inventory = _inventoryServiceRepository.ReadQuantityAndReservations(id).Result;
            _quantity = inventory.Item1;
            _reservedQuantity = inventory.Item2;
            Context.System.Scheduler.ScheduleTellRepeatedly(
                TimeSpan.Zero
                , TimeSpan.FromMilliseconds(100)
                , Self
                , new FlushStreamMessage(_id)
                , ActorRefs.Nobody);
        }

        private void Running()
        {
            Receive<ReserveMessage>(message =>
            {
                var newReservedQuantity = _reservedQuantity + message.ReservationQuantity;
                if (newReservedQuantity <= _quantity)
                {
                    var sender = Sender;
                    _inventoryServiceRepository.WriteQuantityAndReservations(
                        message.ProductId
                        , _quantity
                        , newReservedQuantity)
                        .ContinueWith(task =>
                        {
                            if (task.Result)
                            {
                                _reservedQuantity = newReservedQuantity;
                                return new ReservedMessage(_id, message.ReservationQuantity, true);
                            }
                            else
                            {
                                return new ReservedMessage(_id, message.ReservationQuantity, false);
                            }
                        }, TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
                        .PipeTo(sender);
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
                _inventoryServiceRepository.Flush(_id);
            });
        }
    }
}

