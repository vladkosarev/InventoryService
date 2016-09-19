using Akka.Actor;
using Akka.Util.Internal;
using InventoryService.Messages.Request;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;

namespace InventoryService.Console
{
    public class SampleClientClass
    {
        public async Task StartSampleClientAsync()
        {
            const int productCount = 300;
            const int initialQuantity = 1;

            IList<Tuple<string, int, int>> products = new List<Tuple<string, int, int>>();
            products.Add(new Tuple<string, int, int>("ticketsections-216", initialQuantity, 0));
            products.Add(new Tuple<string, int, int>("ticketsections-217", initialQuantity, 0));
            products.Add(new Tuple<string, int, int>("ticketsections-218", initialQuantity, 0));
            products.Add(new Tuple<string, int, int>("ticketsections-219", initialQuantity, 0));
            products.Add(new Tuple<string, int, int>("ticketsections-215", initialQuantity, 0));
            products.Add(new Tuple<string, int, int>("ticketsections-214", initialQuantity, 0));
            products.Add(new Tuple<string, int, int>("ticketsections-213", initialQuantity, 0));
            products.Add(new Tuple<string, int, int>("ticketsections-212", initialQuantity, 0));
            products.Add(new Tuple<string, int, int>("ticketsections-211", initialQuantity, 0));
            products.Add(new Tuple<string, int, int>("ticketsections-210", initialQuantity, 0));
            products.Add(new Tuple<string, int, int>("ticketsections-209", initialQuantity, 0));

            System.Console.WriteLine("Starting Client");
            var actorSystem = ActorSystem.Create("InventoryService-Client");
            {
                var remoteAddress = ConfigurationManager.AppSettings["RemoteActorAddress"];
                var inventoryActorSelection =
                    actorSystem.ActorSelection(remoteAddress);

                var inventoryActor = inventoryActorSelection;

                var stopwatch = new Stopwatch();

                stopwatch.Start();

                products.ForEach(p =>
                {
                    for (var i = 0; i < 1; i++)
                    {
                        try
                        {
                            inventoryActor.Ask(new UpdateQuantityMessage(p.Item1, 5000)).Wait();
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine("Failed on iteration {0} while updating quantity {1} : {2}", i,
                                p.Item1,
                                ex.Message + " - " + ex);
                        }

                        //     inventoryActor.Tell(new ReserveMessage(p.Item1, 1));
                        //    System.Console.WriteLine("Updating  item {0}'s quantity for product {0}", i, p.Item1);

                        //   await Task.Delay(TimeSpan.FromSeconds(1));
                    }
                });

                //try
                //{
                //    var x = await inventoryActor.Ask<IInventoryServiceCompletedMessage>(new PurchaseMessage("ticketsection-216", 2));
                //}
                //catch (Exception e)
                //{
                //}

                // Task.WaitAll(actorSystem.Terminate());
                stopwatch.Stop();

                System.Console.WriteLine("Elapsed: {0}", stopwatch.Elapsed.TotalSeconds);
                System.Console.WriteLine("Speed: {0} per second", productCount * initialQuantity / stopwatch.Elapsed.TotalSeconds);
            }
        }
    }
}