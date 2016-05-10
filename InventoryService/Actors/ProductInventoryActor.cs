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
        private readonly IActorRef inventoryServiceRepositoryActor;
        public IStash Stash { get; set; }

        public ProductInventoryActor(IInventoryServiceRepository inventoryServiceRepository, string id)
        {
            _id = id;
            inventoryServiceRepositoryActor = Context.ActorOf(
                Props.Create(() =>
                    new ProductInventoryRepositoryActor(inventoryServiceRepository, _id))
                    , _id + "-repository");
            Become(Initializing);
            inventoryServiceRepositoryActor.Tell(new GetInventoryMessage());
        }

        private void Initializing()
        {
            Receive<LoadedInventoryMessage>(message =>
            {
                _quantity = message.Quantity;
                _reservedQuantity = message.ReservedQuantity;
                Become(Running);
                Stash.UnstashAll();
            });

            ReceiveAny(message =>
            {
                Stash.Stash();
            });
        }

        private void Running()
        {
            ReceiveAsync<ReserveMessage>(async message =>
            {
                var newReservedQuantity = _reservedQuantity + message.ReservationQuantity;
                if (newReservedQuantity <= _quantity)
                {
                    // write to repository here
                    //var result = await inventoryServiceRepositoryActor.Ask<WroteReservationsMessage>(
                    //    new WriteReservationsMessage(_id, newReservedQuantity));
                    var result = await inventoryServiceRepositoryActor.Ask<WroteInventoryMessage>(
                        new WriteInventoryMessage(_id, _quantity, newReservedQuantity));
                    if (result.Successful)
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
        }
    }
}

