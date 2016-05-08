using System;
using Akka.Actor;
using InventoryService.Repository;

namespace InventoryService
{
	public class ProductInventoryActor : ReceiveActor, IWithUnboundedStash
	{
		private string Id;
		private int Quantity;
		private int ReservedQuantity;
		private IActorRef inventoryServiceRepositoryActor;
		public IStash Stash { get; set; }

		public ProductInventoryActor (IInventoryServiceRepository inventoryServiceRepository, string id)
		{
			Id = id;
			inventoryServiceRepositoryActor = Context.ActorOf(
				Props.Create(() => 
					new ProductInventoryRepositoryActor(inventoryServiceRepository, Id))
					, Id + "-repository");
			Become (Initializing);
			inventoryServiceRepositoryActor.Tell(new GetInventoryMessage());
		}

		private void Initializing()
		{
			Receive<LoadedInventoryMessage> (message => {
				Quantity = message.Quantity;
				ReservedQuantity = message.ReservedQuantity;
				Become(Running);
				Stash.UnstashAll();
			});

			ReceiveAny(message => {
				Stash.Stash();
			});
		}

		private void Running()
		{
			Receive<ReserveMessage> (message => {
				var newReservedQuantity = ReservedQuantity + message.ReservationQuantity;
				if (newReservedQuantity <= Quantity ) {
					// write to repository here
					ReservedQuantity = newReservedQuantity;
					Sender.Tell(new ReservedMessage(Id, message.ReservationQuantity, true));
				} else {
					Sender.Tell(new ReservedMessage(Id, message.ReservationQuantity, false));
				}
			});
		}
	}
}

