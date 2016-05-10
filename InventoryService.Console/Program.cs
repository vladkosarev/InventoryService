using System;
using System.Diagnostics;
using InventoryService;
using InventoryService.Repository;
using InventoryService.Actors;
using InventoryService.Messages;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;

namespace InventoryService.Console
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			var inventoryService = new FileServiceRepository();

			Task.Run(() => inventoryService.WriteQuantityAndReservations("product1", 10000, 0)).Wait();
			Task.Run(() => inventoryService.WriteQuantityAndReservations("product2", 10000, 0)).Wait();

			var sys = ActorSystem.Create("TestSystem");

			var inventoryActor = sys.ActorOf(Props.Create(() => new InventoryActor(inventoryService)));

			var stopwatch = new Stopwatch();

			stopwatch.Start ();

			var task1 = Task.Run(() => {
				for (var i = 0; i < 10000; i++) {
					var reservation = inventoryActor.Ask<ReservedMessage>(new ReserveMessage ("product1", 1)).Result;
					if (!reservation.Successful)
						System.Console.WriteLine ("Failed on iteration {0}", i);
				}});

            var task2 = Task.Run(() =>
            {
                for (var i = 0; i < 10000; i++)
                {
                    var reservation = inventoryActor.Ask<ReservedMessage>(new ReserveMessage("product2", 1)).Result;
                    if (!reservation.Successful)
                        System.Console.WriteLine("Failed on iteration {0}", i);
                }
            });

            Task.WhenAll(task1, task2).Wait();
            //Task.WhenAll(task1).Wait();

			stopwatch.Stop ();
			System.Console.WriteLine("Elapsed: {0}", stopwatch.Elapsed.TotalSeconds);
			System.Console.ReadLine ();
		}
	}
}
