Thank you for downloading inventory service server!

Here are a few code snippets to get you started

YOU CAN USE THE SERVER ACTOR SYSTEM AUTO-SET INITIAL INVENTORY
==============================================================

            var initialInventory = new RealTimeInventory("ticketsections-100", 10, 0, 0);
            using (var server = new InventoryServiceAkkaInMemory(initialInventory))
            {
                //Using the lib actor system
                var result = server.inventoryActor.Ask<IInventoryServiceCompletedMessage>(new ReserveMessage(initialInventory.ProductId, 20)).Result;
                if (result.Successful)
                {
                    Console.WriteLine(result.RealTimeInventory);
                }
                else
                {
                    Console.WriteLine(result.RealTimeInventory);
                }
            }


YOU CAN USE THE SERVER ACTOR SYSTEM WITHOUT AUTO-SET INITIAL INVENTORY
=====================================================================


            using (var server = new InventoryServiceAkkaInMemory())
            {
                //Using the lib actor system
                var result = server.inventoryActor.Ask<IInventoryServiceCompletedMessage>(new ReserveMessage(initialInventory.ProductId, 20)).Result;
                if (result.Successful)
                {
                    Console.WriteLine(result.RealTimeInventory);
                }
                else
                {
                    Console.WriteLine(result.RealTimeInventory);
                }
            }


YOU CAN USE YOUR ACTOR SYSTEM
=============================

            using (var server = new InventoryServiceAkkaInMemory())
            {
                var mySystem = Akka.Actor.ActorSystem.Create("mySystem");
                var address = ConfigurationManager.AppSettings["RemoteActorAddress"];
                var inventoryActor = mySystem.ActorSelection(address);

                var result = inventoryActor.Ask<IInventoryServiceCompletedMessage>(new ReserveMessage(initialInventory.ProductId, 20)).Result;
                if (result.Successful)
                {
                    Console.WriteLine(result.RealTimeInventory);
                }
                else
                {
                    Console.WriteLine(result.RealTimeInventory);
                }
            }