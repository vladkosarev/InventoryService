using System;
using System.Collections.Generic;
using Akka.Actor;

namespace InventoryService
{
	public class InventoryActor : ReceiveActor
	{
		private Dictionary<string, IActorRef> products = new Dictionary<string, IActorRef>();

		public InventoryActor()
		{
			Receive<ReserveMessage>(message =>
				{
					if (!products.ContainsKey(message.ProductId)) {
						var productActorRef = Context.ActorOf(
							Props.Create(() => new ProductInventoryActor(message.ProductId)), message.ProductId);
						products.Add(message.ProductId, productActorRef);
					}
					products[message.ProductId].Forward(message);
				});
		}
	}
}

