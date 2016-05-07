using System;
using Akka.Actor;
using InventoryService.Repository;

namespace InventoryService
{
	public class ProductInventoryActor : ReceiveActor, IWithUnboundedStash
	{
		private string Id;
		private int Quantity;
		private int Reservations;
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
				Reservations = message.Reservations;
				Become(Running);
				Stash.UnstashAll();
			});

			Receive<ReserveMessage> (message => {
				Stash.Stash();
			});
		}

		private void Running()
		{
			Receive<ReserveMessage> (message => {
				var newQuantity = Quantity - message.Quantity;
				if (newQuantity >= 0) {
					// write to repository here
					Quantity = newQuantity;
					Sender.Tell(new ReservedMessage(Id, Quantity, true));
				} else {
					Sender.Tell(new ReservedMessage(Id, Quantity, false));
				}
			});
		}
	}
}

