using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using InventoryService.Actors;
using InventoryService.Messages;
using InventoryService.Storage;

namespace InventoryService.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing");

            var storageType = Type.GetType(ConfigurationManager.AppSettings["Storage"]);

            const int productCount = 3000;
            const int initialQuantity = 10000;

            IList<Tuple<string, int, int>> products = new List<Tuple<string, int, int>>();
            for (var product = 0; product < productCount; product++)
            {
                products.Add(new Tuple<string, int, int>("products" + product, initialQuantity, 0));
            }

            var inventoryService = (IInventoryStorage)Activator.CreateInstance(storageType);

            //using (var service = (IInventoryStorage)Activator.CreateInstance(storageType))
            //{
                Task.WaitAll(
                    products
                    .Select(p => inventoryService.WriteInventory(p.Item1, p.Item2, p.Item3))
                    .ToArray());
            //}
            
            Console.WriteLine("Starting Server");

            // close and re-open

            using (var actorSystem = ActorSystem.Create("InventoryService-Server"))
            {
                var inventoryActor = actorSystem.ActorOf(Props.Create(() => new InventoryActor(inventoryService, true)), "InventoryActor");

 
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


            Console.ReadLine();
            }
        }
    }
}
