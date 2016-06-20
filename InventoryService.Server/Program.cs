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
                var inventoryActor = actorSystem.ActorOf(Props.Create(() => new InventoryActor(inventoryService, new TestPerformanceService(), true)), "InventoryActor");

                Console.ReadLine();
            }
        }
    }
}
