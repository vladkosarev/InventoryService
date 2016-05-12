using System;
using System.Threading.Tasks;
using Akka.Actor;
using InventoryService.Messages;
using InventoryService.Repository;

namespace InventoryService.Actors
{
    public class ProductInventoryActor : ReceiveActor//, IWithUnboundedStash
    {
        private readonly string _id;
        private int _quantity;
        private int _reservedQuantity;
        private readonly IActorRef inventoryServiceRepositoryActor;
        //public IStash Stash { get; set; }
        private readonly IInventoryServiceRepository _inventoryServiceRepository;

        public ProductInventoryActor(IInventoryServiceRepository inventoryServiceRepository, string id, int quantity = 0, int reservedQuantity = 0)
        {
            _id = id;
            //inventoryServiceRepositoryActor = Context.ActorOf(
            //    Props.Create(() =>
            //        new ProductInventoryRepositoryActor(inventoryServiceRepository, _id))
            //        , _id + "-repository");
            //Become(Initializing);
            //inventoryServiceRepositoryActor.Tell(new GetInventoryMessage());
            _inventoryServiceRepository = inventoryServiceRepository;
            Tuple<int, int> inventory;
            try
            {
                //inventory = _inventoryServiceRepository.ReadQuantityAndReservations(_id).Result;
                //Console.WriteLine("Initialized {0}", id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
            
            //_quantity = inventory.Item1;
            //_reservedQuantity = inventory.Item2;
            _quantity = quantity;
            _reservedQuantity = reservedQuantity;
            Become(Running);
        }

        //private void Initializing()
        //{
        //    Receive<LoadedInventoryMessage>(message =>
        //    {
        //        _quantity = message.Quantity;
        //        _reservedQuantity = message.ReservedQuantity;
        //        Become(Running);
        //        Stash.UnstashAll();
        //    });

        //    ReceiveAny(message =>
        //    {
        //        Stash.Stash();
        //    });
        //}

        private void Running()
        {
            Receive<ReserveMessage>(message =>
            {
                var newReservedQuantity = _reservedQuantity + message.ReservationQuantity;
                if (newReservedQuantity <= _quantity)
                {
                    // write to repository here
                    //var result = await inventoryServiceRepositoryActor.Ask<WroteReservationsMessage>(
                    //    new WriteReservationsMessage(_id, newReservedQuantity));
                    //var result = await inventoryServiceRepositoryActor.Ask<WroteInventoryMessage>(
                    //    new WriteInventoryMessage(_id, _quantity, newReservedQuantity));

                    var sender = Sender;
                    _inventoryServiceRepository.WriteQuantityAndReservations(_id, _quantity, newReservedQuantity)
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
                        },
                        TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
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
        }
    }
}

