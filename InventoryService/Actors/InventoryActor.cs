using System;
using System.Collections.Generic;
using Akka.Actor;
using InventoryService.Messages;
using InventoryService.Repository;

namespace InventoryService.Actors
{
	public class InventoryActor : ReceiveActor
	{
		private readonly Dictionary<string, IActorRef> _products = new Dictionary<string, IActorRef>();

		public InventoryActor(IInventoryServiceRepository inventoryServiceRepository)
		{
			Receive<ReserveMessage>(message =>
				{
					if (!_products.ContainsKey(message.ProductId)) {
						var productActorRef = Context.ActorOf(
							Props.Create(() => 
								new ProductInventoryActor(inventoryServiceRepository, message.ProductId))
								, message.ProductId);
						_products.Add(message.ProductId, productActorRef);
					}                    
					_products[message.ProductId].Forward(message);
				});
			
			Receive<PurchaseMessage>(message =>
				{
					if (!_products.ContainsKey(message.ProductId)) {
						var productActorRef = Context.ActorOf(
							Props.Create(() => 
								new ProductInventoryActor(inventoryServiceRepository, message.ProductId))
							, message.ProductId);
						_products.Add(message.ProductId, productActorRef);
					}
					_products[message.ProductId].Forward(message);
				});
		}
	}
}

