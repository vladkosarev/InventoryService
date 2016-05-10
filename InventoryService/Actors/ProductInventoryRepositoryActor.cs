using System.Threading.Tasks;
using Akka.Actor;
using InventoryService.Messages;
using InventoryService.Repository;

namespace InventoryService.Actors
{
	public class ProductInventoryRepositoryActor : ReceiveActor
	{
		private readonly IInventoryServiceRepository _inventoryServiceRepository;
		private readonly string _id;
        //private ITellScheduler _messageScheduler = new DedicatedThreadScheduler(Context.System);

        public ProductInventoryRepositoryActor (IInventoryServiceRepository inventoryServiceRepository, string id)
		{
			_inventoryServiceRepository = inventoryServiceRepository;
			_id = id;
            //_messageScheduler.ScheduleTellRepeatedly(0, 100, Self, new FlushStreamsMessage(), Self);

			Receive<GetInventoryMessage> (message => {
				_inventoryServiceRepository.ReadQuantityAndReservations(_id)
					.ContinueWith(task => new LoadedInventoryMessage(task.Result.Item1, task.Result.Item2), 
						TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
					.PipeTo(Sender);
			});

            ReceiveAsync<WriteReservationsMessage>(async message =>
            {
                var success = await _inventoryServiceRepository.WriteReservations(_id, message.ReservationQuantity);
                Sender.Tell(new WroteReservationsMessage(success));
            });

            ReceiveAsync<WriteInventoryMessage> (async message =>
			{
			    var success = await _inventoryServiceRepository.WriteQuantityAndReservations(_id, message.Quantity, message.ReservationQuantity);
			    Sender.Tell(new WroteInventoryMessage(success));			    
			});

            ReceiveAsync<FlushStreamsMessage>(async message =>
            {
                await _inventoryServiceRepository.Flush();
            });
        }
    }
}

