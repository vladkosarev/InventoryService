using System.Threading.Tasks;
using Akka.Actor;
using InventoryService.Messages;
using InventoryService.Repository;

namespace InventoryService.Actors
{
	public class ProductInventoryRepositoryActor : ReceiveActor
	{
		private readonly IInventoryServiceRepository _inventoryServiceRepository;
		private readonly string Id;

		public ProductInventoryRepositoryActor (IInventoryServiceRepository inventoryServiceRepository, string id)
		{
			_inventoryServiceRepository = inventoryServiceRepository;
			Id = id;

			Receive<GetInventoryMessage> (message => {
				_inventoryServiceRepository.ReadQuantityAndReservations(Id)
					.ContinueWith(task => new LoadedInventoryMessage(task.Result.Item1, task.Result.Item2), 
						TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
					.PipeTo(Sender);
			});

			Receive<WriteInventoryMessage> (message => {
				_inventoryServiceRepository.WriteQuantityAndReservations(Id, message.Quantity, message.ReservationQuantity)
					.ContinueWith(task => new WroteInventoryMessage(true), 
						TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
					.PipeTo(Sender);
			});
		}
	}
}

