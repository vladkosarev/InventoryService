using System;
using Akka.Actor;
using System.Threading.Tasks;
using InventoryService.Repository;

namespace InventoryService
{
	public class ProductInventoryRepositoryActor : ReceiveActor
	{
		private IInventoryServiceRepository _inventoryServiceRepository;
		private string Id;

		public ProductInventoryRepositoryActor (IInventoryServiceRepository inventoryServiceRepository, string id)
		{
			_inventoryServiceRepository = inventoryServiceRepository;
			Id = id;

			Receive<GetInventoryMessage> (message => {
				inventoryServiceRepository.ReadQuantityAndReservations(Id)
					.ContinueWith(task => new LoadedInventoryMessage(task.Result.Item1, task.Result.Item2), 
						TaskContinuationOptions.AttachedToParent & TaskContinuationOptions.ExecuteSynchronously)
					.PipeTo(Sender);
			});
		}
	}
}

