using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;
using InventoryService.Actors;
using InventoryService.Messages;
using InventoryService.Storage;

namespace InventoryService.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(
    @"
akka {
    actor {
        serializers {
            wire = ""Akka.Serialization.WireSerializer, Akka.Serialization.Wire""
        }
            serialization-bindings {
                ""System.Object"" = wire
        }
        provider = ""Akka.Cluster.ClusterActorRefProvider, Akka.Cluster""
        debug {  
            receive = on 
            autoreceive = on
            lifecycle = on
            event-stream = on
            unhandled = on
        }
    }

    remote {
                helios.tcp {
                    port = 10000
                    hostname = localhost
                }
            }
            cluster {
                seed-nodes = [""akka.tcp://InventoryServiceCluster@localhost:10000""]
                roles = [""server""]
                auto-down-unreachable-after = 30s
            }
        }
");
            Console.WriteLine("Starting Server");

            var productCount = 100;
            var initialQuantity = 5000;

            IList<Tuple<string, int, int>> products = new List<Tuple<string, int, int>>();
            for (int product = 0; product < productCount; product++)
            {
                products.Add(new Tuple<string, int, int>("product" + product, initialQuantity, 0));
            }

            using (var service = new FileStorage(appendMode: false))
            {
                Task.WaitAll(
                    products
                    .Select(p => service.WriteInventory(p.Item1, p.Item2, p.Item3))
                    .ToArray());
            }

            // close and re-open
            var inventoryService = new FileStorage();

            using (var actorSystem = ActorSystem.Create("InventoryServiceCluster", config))
            {
                var inventoryActor = actorSystem.ActorOf(Props.Create(() => new InventoryActor(inventoryService)), "InventoryActor");
                Console.ReadLine();
            }
        }
    }
}
