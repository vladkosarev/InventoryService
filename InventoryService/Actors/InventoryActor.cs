using System;
using System.Collections.Generic;
using Akka.Actor;
using InventoryService.Repository;

namespace InventoryService
{
	public class InventoryActor : ReceiveActor
	{
		private Dictionary<string, IActorRef> products = new Dictionary<string, IActorRef>();

		public InventoryActor(IInventoryServiceRepository inventoryServiceRepository)
		{
			Receive<ReserveMessage>(message =>
				{
					if (!products.ContainsKey(message.ProductId)) {
						var productActorRef = Context.ActorOf(
							Props.Create(() => 
								new ProductInventoryActor(inventoryServiceRepository, message.ProductId))
								, message.ProductId);
						products.Add(message.ProductId, productActorRef);
					}
					products[message.ProductId].Forward(message);
				});
		}
	}
}

