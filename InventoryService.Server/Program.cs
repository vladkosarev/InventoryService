using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using InventoryService.Actors;
using InventoryService.Storage;

namespace InventoryService.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Initializing");

            var storageType = Type.GetType(ConfigurationManager.AppSettings["Storage"]);
            if (storageType == null)
            {
                Console.WriteLine("Invalid Storage Type {0}", ConfigurationManager.AppSettings["Storage"]);
                return;
            } 

            const int productCount = 3000;
            const int initialQuantity = 10000;

            var products = new List<Tuple<string, int, int, int>>();
            for (var product = 0; product < productCount; product++)
            {
                products.Add(new Tuple<string, int, int, int>("products" + product, initialQuantity, 0, 0));
            }

            var inventoryService = (IInventoryStorage)Activator.CreateInstance(storageType);

            //using (var service = (IInventoryStorage)Activator.CreateInstance(storageType))
            //{
            Task.WaitAll(
                products
                .Select(p => inventoryService.WriteInventory(p.Item1, p.Item2, p.Item3, p.Item4))
                .ToArray());
            //}

            Console.WriteLine("Starting Server");

            // close and re-open

            using (var actorSystem = ActorSystem.Create("InventoryService-Server"))
            {
                var inventoryActor = actorSystem.ActorOf(Props.Create(() => new InventoryActor(inventoryService, new ConsolePerformanceService(), true)), "InventoryActor");

                Console.ReadLine();
            }
        }
    }
}
