using Akka.Actor;
using InventoryService.Actors;
using InventoryService.ActorSystemFactoryLib;
using InventoryService.Storage;
using NLog;
using System;
using System.Configuration;

namespace InventoryService.Server
{
    public class InventoryServiceServerApp
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public InventoryServiceServerApp()
        {
            ActorSystemFactory = new ActorSystemFactory();
        }

        private ActorSystemFactory ActorSystemFactory { set; get; }

        public void StartServer(IPerformanceService performanceService, Action<IActorRef, ActorSystem> onReady = null, Type storageType = null, string serverActorSystemName = null,
            ActorSystem serverActorSystem = null, string serverActorSystemConfig = null)
        {
            Log.Debug("Initializing ...");
            IActorRef inventoryActor = null;

            if (storageType == null)
            {
                // var message = "Unable to initialize storage. No storage specified" ;
                // Log.Debug(message);
                var storageSettings = ConfigurationManager.AppSettings["Storage"];
                if (string.IsNullOrEmpty(storageSettings))
                {
                    const string message = "Could not find Storage setup in config";
                    Log.Debug(message);
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
                Log.Debug(message);
                throw new Exception(message);
            }
            var inventoryStorage = (IInventoryStorage)Activator.CreateInstance(storageType);

            Log.Debug("Starting Server");

            ActorSystemFactory.CreateOrSetUpActorSystem(serverActorSystemName: serverActorSystemName,
                actorSystem: serverActorSystem, actorSystemConfig: serverActorSystemConfig);

            inventoryActor =
               ActorSystemFactory.InventoryServiceActorSystem.ActorOf(
                   Props.Create(() => new InventoryActor(inventoryStorage, performanceService, true)),
                   typeof(InventoryActor).Name);

            if (inventoryActor == null || inventoryActor.IsNobody())
            {
                const string message = "Unable to create actor";
                Log.Debug(message);
                throw new Exception(message);
            }
            ActorSystem = ActorSystemFactory.InventoryServiceActorSystem;

            onReady?.Invoke(inventoryActor, ActorSystem);
        }

        public ActorSystem ActorSystem { get; set; }

        public void StopServer()
        {
            // ActorSystemFactory.TerminateActorSystem();
        }
    }
}