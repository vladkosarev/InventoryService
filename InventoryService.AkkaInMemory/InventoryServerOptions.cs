using Akka.Actor;
using InventoryService.Messages.Models;
using System;
using System.Net;
using System.Net.Sockets;

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

        public Action<IActorRef, ActorSystem> OnInventoryActorSystemReady { set; get; }

        /// <summary>
        /// See http://getakka.net/docs/concepts/configuration for more info
        /// </summary>
        public string ServerActorSystemConfig { get; set; }

        public string ServerActorSystemName { get; set; }
        public bool UseActorSystem { get; set; }

        /// <summary>
        /// Not yet cross platform tested
        /// </summary>
        /// <returns></returns>
        public static int GetFreeTcpPort()
        {
            var p = new TcpListener(IPAddress.Loopback, 0);
            p.Start();
            var port = ((IPEndPoint)p.LocalEndpoint).Port;
            p.Stop();
            return port;
        }
    }
}