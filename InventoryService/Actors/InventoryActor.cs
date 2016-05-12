using System;
using System.Collections.Generic;
using System.Linq;
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
            //TODO this whole init sucks, improve
            var products = inventoryServiceRepository.GetAll().Result;

            foreach (var product in products)
            {
                if (!product.Key.EndsWith("/q")) continue;

                var productId = product.Key.Split('/')[0];
                var productActorRef = Context.ActorOf(
                    Props.Create(() =>
                        new ProductInventoryActor(inventoryServiceRepository, productId, product.Value, products[productId + "/r"]))
                    , productId);
                //Console.WriteLine("Creating - {0} {1} {2}", productId, product.Value, products[productId + "/r"]);
                _products.Add(productId, productActorRef);
            }

            Receive<ReserveMessage>(message =>
                {
                    _products[message.ProductId].Forward(message);
                });

            Receive<PurchaseMessage>(message =>
                {
                    _products[message.ProductId].Forward(message);
                });
        }
    }
}

