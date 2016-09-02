using Akka.Actor;
using InventoryService.Actors;
using InventoryService.Storage;
using System;
using System.Configuration;

namespace InventoryService.Server
{
    public class InventoryServiceServer
    {
        public void StartServer()
        {
            Console.WriteLine("Initializing");
            var actorSystemName = ConfigurationManager.AppSettings["ServerActorSystemName"];

            if (actorSystemName == null)
            {
                const string message = "Invalid ActorSystemName.Please set up 'ServerActorSystemName' in the config file";
                Console.WriteLine(message);

                throw new Exception(message);
            }
            var storageSettings = ConfigurationManager.AppSettings["Storage"];
            if (storageSettings == null)
            {
                const string message = "Invalid storageSettings.Please set up 'Storage' in the config file";
                Console.WriteLine(message);

                throw new Exception(message);
            }

            var storageType = Type.GetType(storageSettings);
            if (storageType == null)
            {
                var message = "Invalid Storage Type " + storageSettings;
                Console.WriteLine(message);

                throw new Exception(message);
            }

            var inventoryService = (IInventoryStorage)Activator.CreateInstance(storageType);

            Console.WriteLine("Starting Server");

            ActorSystem = ActorSystem.Create(actorSystemName);
            var inventoryActor =
                ActorSystem.ActorOf(
                    Props.Create(() => new InventoryActor(inventoryService, new ConsolePerformanceService(), true)),
                    typeof(InventoryActor).Name);

            if (inventoryActor == null || inventoryActor.IsNobody())
            {
                var message = "Unable to create actor " + actorSystemName;
                Console.WriteLine(message);
                throw new Exception(message);
            }
        }

        public void StopServer()
        {
            ActorSystem.Terminate();
            ActorSystem.Dispose();
        }

        public static ActorSystem ActorSystem { get; set; }
    }
}