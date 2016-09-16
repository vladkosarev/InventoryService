using Akka.Actor;
using InventoryService.Messages.Models;
using System;

namespace InventoryService.AkkaInMemoryServer
{
    public class InventoryServerOptions
    {
        public RealTimeInventory InitialInventory { set; get; }

        public ActorSystem ClientActorSystem { set; get; }

        public Type StorageType { set; get; }

        public string InventoryActorAddress { set; get; }

        public string ServerEndPoint { set; get; }

        public ActorSystem ServerActorSystem { get; set; }

        /// <summary>
        /// See http://getakka.net/docs/concepts/configuration for more info
        /// </summary>
        public string ServerActorSystemConfig { get; set; }

        public string ServerActorSystemName { get; set; }
    }
}