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
            const int productCount = 100;
            const int initialQuantity = 5000;

            IList<Tuple<string, int, int>> products = new List<Tuple<string, int, int>>();
            for (var product = 0; product < productCount; product++)
            {
                products.Add(new Tuple<string, int, int>("products" + product, initialQuantity, 0));
            }

            System.Console.WriteLine("Starting Client");
            using (var actorSystem = ActorSystem.Create("InventoryService-Client"))
            {
                var inventoryActorSelection =
                    actorSystem.ActorSelection("akka.tcp://InventoryService-Server@localhost:10000/user/InventoryActor");

                var inventoryActor = inventoryActorSelection.ResolveOne(TimeSpan.FromSeconds(30)).Result;

                var stopwatch = new Stopwatch();

                stopwatch.Start();

                Task.WaitAll(products.Select(p =>
                {
                    return Task.Run(async () =>
                    {
                        for (var i = 0; i < initialQuantity; i++)
                        {
                            var reservation = await inventoryActor.Ask<ReservedMessage>(new ReserveMessage(p.Item1, 1), TimeSpan.FromSeconds(10));
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
