using Akka.Actor;
using InventoryService.Messages.Request;
using InventoryService.Messages.Response;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Akka.Util.Internal;

namespace InventoryService.Console
{
    public class SampleClientClass
    {
        public async Task StartSampleClientAsync()
        {
            const int productCount = 1000;
            const int initialQuantity = 100000;

            IList<Tuple<string, int, int>> products = new List<Tuple<string, int, int>>();
            for (var product = 0; product < productCount; product++)
            {
                products.Add(new Tuple<string, int, int>("products" + product, initialQuantity, 0));
            }

            System.Console.WriteLine("Starting Client");
            var actorSystem = ActorSystem.Create("InventoryService-Client");
            {
                var remoteAddress = ConfigurationManager.AppSettings["RemoteActorAddress"];
                var inventoryActorSelection =
                    actorSystem.ActorSelection(remoteAddress);

                var inventoryActor = inventoryActorSelection;

                var stopwatch = new Stopwatch();

                stopwatch.Start();

                    products.ForEach( p =>
                {
                    for (var i = 0; i < initialQuantity; i++)
                    {
                        try
                        {
                            inventoryActor.Tell(new UpdateQuantityMessage(p.Item1, 1));
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine("Failed on iteration {0} while updating quantity {1} : {2}", i,
                                p.Item1,
                                ex.Message + " - " + ex);
                        }

                        inventoryActor.Tell(new ReserveMessage(p.Item1, 1));
                        System.Console.WriteLine("Updating  item {0}'s quantity for product {0}", i, p.Item1);

                        //   await Task.Delay(TimeSpan.FromSeconds(1)); 
                    }
                });
           
                Task.WaitAll(products.Select(async p =>
            {
                for (var i = 0; i < initialQuantity; i++)
                {
                    try
                    {
                        var update =
                            await
                                inventoryActor.Ask<UpdateQuantityCompletedMessage>(
                                    new UpdateQuantityMessage(p.Item1, 1), TimeSpan.FromSeconds(10)).ConfigureAwait(false);
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
                                TimeSpan.FromSeconds(10)).ConfigureAwait(false);
                    System.Console.WriteLine(
                        !reservation.Successful ? "Failed on iteration {0}" : "Reserved item {0} successfully ", i);
                }

                    // await Task.Delay(TimeSpan.FromSeconds(10));
                }).ToArray());
                // Task.WaitAll(actorSystem.Terminate());
                stopwatch.Stop();

                System.Console.WriteLine("Elapsed: {0}", stopwatch.Elapsed.TotalSeconds);
                System.Console.WriteLine("Speed: {0} per second", productCount * initialQuantity / stopwatch.Elapsed.TotalSeconds);
            }
        }
    }
}