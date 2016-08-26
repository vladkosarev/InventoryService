using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using InventoryService.Messages;

namespace InventoryService.Console
{
    public class SampleClientClass
    {
        public async Task StartSampleClientAsync()
        {
            const int productCount = 10;
            const int initialQuantity = 50;

            IList<Tuple<string, int, int>> products = new List<Tuple<string, int, int>>();
            for (var product = 0; product < productCount; product++)
            {
                products.Add(new Tuple<string, int, int>("products" + product, initialQuantity, 0));
            }

            System.Console.WriteLine("Starting Client");
            using (var actorSystem = ActorSystem.Create("InventoryService-Client"))
            {
                var remoteAddress = ConfigurationManager.AppSettings["RemoteActorAddress"];
                var inventoryActorSelection =
                    actorSystem.ActorSelection(remoteAddress);

                var inventoryActor = inventoryActorSelection;

                var stopwatch = new Stopwatch();

                stopwatch.Start();

                Task.WaitAll(products.Select(p =>
                {
                    return Task.Run(async () =>
                    {
                        for (var i = 0; i < initialQuantity; i++)
                        {
                            try
                            {
                                var update =
                                    await
                                        inventoryActor.Ask<UpdateQuantityCompletedMessage>(
                                            new UpdateQuantityMessage(p.Item1, 1), TimeSpan.FromSeconds(10));
                                if (update == null) throw new Exception("remote actor returned null message");
                                System.Console.WriteLine("Updating  item {0}'s quantity for product {0}", i, p.Item1);
                            }
                            catch (Exception ex)
                            {
                                System.Console.WriteLine("Failed on iteration {0} while updating quantity {1} : {2}", i, p.Item1,
                                    ex.Message + " - " + ex);
                            }

                            var reservation =
                                await
                                    inventoryActor.Ask<ReserveCompletedMessage>(new ReserveMessage(p.Item1, 1),
                                        TimeSpan.FromSeconds(10));
                            System.Console.WriteLine(
                                !reservation.Successful ? "Failed on iteration {0}" : "Reserved item {0} successfully ", i);
                        }
                    });
                }).ToArray());

                stopwatch.Stop();

                System.Console.WriteLine("Elapsed: {0}", stopwatch.Elapsed.TotalSeconds);
                System.Console.WriteLine("Speed: {0} per second", productCount * initialQuantity / stopwatch.Elapsed.TotalSeconds);


            }
        }
    }
}