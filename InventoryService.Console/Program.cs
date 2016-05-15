using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using InventoryService.Repository;
using InventoryService.Actors;
using InventoryService.Messages;
using System.Threading.Tasks;
using Akka.Actor;
using Akka.Configuration;

namespace InventoryService.Console
{
    class MainClass
    {
        private static IInventoryServiceRepository inventoryService;
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(OnProcessExit);
            var productCount = 10;
            var initialQuantity = 5000;

            IList<Tuple<string, int, int>> products = new List<Tuple<string, int, int>>();
            for (int product = 0; product < productCount; product++)
            {
                products.Add(new Tuple<string, int, int>("product" + product, initialQuantity, 0));
            }

            using (var service = new FileServiceRepository(appendMode: false))
            {
                Task.WaitAll(
                    products
                    .Select(p => service.WriteQuantityAndReservations(p.Item1, p.Item2, p.Item3))
                    .ToArray());
            }

            // close and re-open
            inventoryService = new FileServiceRepository();
            var config = ConfigurationFactory.ParseString(@"akka {  
    stdout-loglevel = WARNING
    loglevel = WARNING
    log-config-on-start = on        
    actor {                
        debug {  
              receive = on 
              autoreceive = on
              lifecycle = on
              event-stream = on
              unhandled = on
        }
    }
            ");
            var sys = ActorSystem.Create("TestSystem", config);

            var inventoryActor = sys.ActorOf(Props.Create(() => new InventoryActor(inventoryService)));

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            Task.WaitAll(products.Select(p =>
            {
                return Task.Run(async () =>
                {
                    for (var i = 0; i < initialQuantity; i++)
                    {
                        var reservation = await inventoryActor.Ask<ReservedMessage>(new ReserveMessage(p.Item1, 1));
                        //System.Console.WriteLine("!{0}", p.Item1);
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

        private static void OnProcessExit(object sender, EventArgs e)
        {
            System.Console.WriteLine("Exiting");
            inventoryService.Flush();
        }
    }
}
