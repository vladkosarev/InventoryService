using Akka.Actor;
using InventoryService.Actors;
using InventoryService.Storage;
using System;
using System.Configuration;
using InventoryService.ActorSystemFactoryLib;

namespace InventoryService.Server
{
    public class InventoryServiceServer
    {
        public void StartServer()
        {
            Console.WriteLine("Initializing");
           
         
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
            ActorSystemFactory.CreateNewActorSystem();
           
            var inventoryActor =
                ActorSystemFactory.InventoryServiceActorSystem.ActorOf(
                    Props.Create(() => new InventoryActor(inventoryService, new ConsolePerformanceService(), true)),
                    typeof(InventoryActor).Name);

            if (inventoryActor == null || inventoryActor.IsNobody())
            {
                var message = "Unable to create actor " ;
                Console.WriteLine(message);
                throw new Exception(message);
            }
        }

        public void StopServer()
        {
            ActorSystemFactory.TerminateActorSystem();
        }

       
    }
}