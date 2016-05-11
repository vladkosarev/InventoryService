using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using InventoryService.Repository;
using InventoryService.Actors;
using InventoryService.Messages;
using System.Threading.Tasks;
using Akka.Actor;

namespace InventoryService.Console
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var inventoryService = new FileServiceRepository();
            var productCount = 10;

            IList<Tuple<string, int, int>> products = new List<Tuple<string, int, int>>();
            for (int product = 0; product < productCount; product++)
            {
                products.Add(new Tuple<string, int, int>("product" + product, 5000, 0));
            }

            Task.WaitAll(
                products
                .Select(p => inventoryService.WriteQuantityAndReservations(p.Item1, p.Item2, p.Item3))
                .ToArray());

            var sys = ActorSystem.Create("TestSystem");

            var inventoryActor = sys.ActorOf(Props.Create(() => new InventoryActor(inventoryService)));

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            Task.WaitAll(products.Select(p =>
            {
                return Task.Run(async () =>
                {
                    for (var i = 0; i < 5000; i++)
                    {
                        var reservation = await inventoryActor.Ask<ReservedMessage>(new ReserveMessage(p.Item1, 1));
                        if (!reservation.Successful)
                            System.Console.WriteLine("Failed on iteration {0}", i);
                    }
                });
            }).ToArray());

            stopwatch.Stop();
            System.Console.WriteLine("Elapsed: {0}", stopwatch.Elapsed.TotalSeconds);
            System.Console.ReadLine();
        }
    }
}
