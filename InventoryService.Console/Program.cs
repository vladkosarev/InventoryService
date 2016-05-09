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

			Task.Run(() => inventoryService.WriteQuantityAndReservations("product1", 100000, 0)).Wait();
			Task.Run(() => inventoryService.WriteQuantityAndReservations("product2", 100000, 0)).Wait();

			ActorSystem Sys = ActorSystem.Create("TestSystem");

			var inventoryActor = Sys.ActorOf(Props.Create(() => new InventoryActor(inventoryService)));

			var stopwatch = new Stopwatch();

			stopwatch.Start ();

			var task1 = Task.Run(() => {
				for (int i = 0; i < 100000; i++) {
					var reservation = inventoryActor.Ask<ReservedMessage>(new ReserveMessage ("product1", 1)).Result;
					if (!reservation.Successful)
						System.Console.WriteLine ("Failed on iteration {0}", i);
				}});
			
			var task2 = Task.Run(() => {
				for (int i = 0; i < 100000; i++) {
					var reservation = inventoryActor.Ask<ReservedMessage>(new ReserveMessage ("product2", 1)).Result;
					if (!reservation.Successful)
						System.Console.WriteLine ("Failed on iteration {0}", i);
				}});

			Task.WhenAll(task1, task2).Wait();

			stopwatch.Stop ();
			System.Console.WriteLine("Elapsed: {0}", stopwatch.Elapsed.TotalSeconds);
			System.Console.ReadLine ();
		}
	}
}
