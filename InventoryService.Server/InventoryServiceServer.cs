using Akka.Actor;
using InventoryService.Actors;
using InventoryService.ActorSystemFactoryLib;
using InventoryService.Storage;
using System;
using System.Configuration;

namespace InventoryService.Server
{
    public class InventoryServiceServerApp
    {
        public void StartServer(Action<IActorRef,ActorSystem> onReady=null, Type storageType = null, string serverActorSystemName = null,
            ActorSystem serverActorSystem = null, string serverActorSystemConfig = null)
        {
            Console.WriteLine("Initializing");

            if (storageType == null)
            {
                // var message = "Unable to initialize storage. No storage specified" ;
                //  Console.WriteLine(message);
                var storageSettings = ConfigurationManager.AppSettings["Storage"];
                if (string.IsNullOrEmpty(storageSettings))
                {
                    const string message = "Could not find Storage setup in config";
                    Console.WriteLine(message);
                    throw new Exception(message);
                }
                else
                {
                    storageType = Type.GetType(storageSettings);
                }
            }
            if (storageType == null)
            {
                const string message = "Could not create storage type";
                Console.WriteLine(message);
                throw new Exception(message);
            }
            var inventoryService = (IInventoryStorage) Activator.CreateInstance(storageType);

            Console.WriteLine("Starting Server");
            ActorSystemFactory.CreateOrSetUpActorSystem(serverActorSystemName: serverActorSystemName,
                actorSystem: serverActorSystem, actorSystemConfig: serverActorSystemConfig);

            var inventoryActor =
                ActorSystemFactory.InventoryServiceActorSystem.ActorOf(
                    Props.Create(() => new InventoryActor(inventoryService, true)),
                    typeof(InventoryActor).Name);

            if (inventoryActor == null || inventoryActor.IsNobody())
            {
                const string message = "Unable to create actor";
                Console.WriteLine(message);
                throw new Exception(message);
            }
            ActorSystem = ActorSystemFactory.InventoryServiceActorSystem;

            onReady?.Invoke(inventoryActor, ActorSystem);
        }

        public ActorSystem ActorSystem { get; set; }

        public void StopServer()
        {
            ActorSystemFactory.TerminateActorSystem();
        }
    }
}