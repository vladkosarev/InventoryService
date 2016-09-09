using System;
using System.Configuration;
using Akka.Actor;

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

            try
            {
                InventoryServiceActorSystem = Akka.Actor.ActorSystem.Create(actorSystemName);
            }
            catch (Exception e)
            {
           var exception= (e as AggregateException).Flatten() ;
                throw;
            }
        }

        public static ActorSystem InventoryServiceActorSystem { get; set; }

        public static void TerminateActorSystem()
        {
            InventoryServiceActorSystem.Terminate();
            InventoryServiceActorSystem.Dispose();
        }
    }
}
