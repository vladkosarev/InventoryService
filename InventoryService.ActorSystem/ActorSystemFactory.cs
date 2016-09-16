using Akka.Actor;
using System;
using System.Configuration;

namespace InventoryService.ActorSystemFactoryLib
{
    public class ActorSystemFactory
    {
        public static void CreateNewActorSystem()
        {
            var actorSystemName = ConfigurationManager.AppSettings["ServerActorSystemName"];

            if (string.IsNullOrEmpty(actorSystemName))
            {
                const string message =
                    "Invalid ActorSystemName.Please set up 'ServerActorSystemName' in the config file";
                Console.WriteLine(message);
                throw new Exception(message);
            }

            InventoryServiceActorSystem = Akka.Actor.ActorSystem.Create(actorSystemName);
        }

        public static ActorSystem InventoryServiceActorSystem { get; set; }

        public static void TerminateActorSystem()
        {
            InventoryServiceActorSystem.Terminate();
            InventoryServiceActorSystem.Dispose();
        }
    }
}