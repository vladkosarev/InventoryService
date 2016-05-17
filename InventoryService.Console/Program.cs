using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using InventoryService.Messages;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;

namespace InventoryService.Console
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var productCount = 10;
            var initialQuantity = 5000;

            IList<Tuple<string, int, int>> products = new List<Tuple<string, int, int>>();
            for (int product = 0; product < productCount; product++)
            {
                products.Add(new Tuple<string, int, int>("product" + product, initialQuantity, 0));
            }

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
                    port = 0
                    hostname = localhost
                }
            }
            cluster {
                seed-nodes = [""akka.tcp://InventoryServiceCluster@localhost:10000""]
                roles = [""client""]
            }
        }
");
            System.Console.WriteLine("Starting Client");
            using (var actorSystem = ActorSystem.Create("InventoryServiceCluster", config))
            {                
                var inventoryActorSelection =
                    actorSystem.ActorSelection("akka.tcp://InventoryServiceCluster@localhost:10000/user/InventoryActor");

                var inventoryActor = inventoryActorSelection.ResolveOne(TimeSpan.FromSeconds(10)).Result;

                var stopwatch = new Stopwatch();

                stopwatch.Start();

                Task.WaitAll(products.Select(p =>
                {
                    return Task.Run(async () =>
                    {
                        for (var i = 0; i < initialQuantity; i++)
                        {
                            var reservation = await inventoryActor.Ask<ReservedMessage>(new ReserveMessage(p.Item1, 1));                            
                            if (!reservation.Successful)
                                System.Console.WriteLine("Failed on iteration {0}", i);                            
                        }
                    });
                }).ToArray());

                stopwatch.Stop();

                System.Console.WriteLine("Elapsed: {0}", stopwatch.Elapsed.TotalSeconds);
                System.Console.WriteLine("Speed: {0} per second", productCount * initialQuantity / stopwatch.Elapsed.TotalSeconds);

                System.Console.ReadLine();                
            }
        }
    }
}
