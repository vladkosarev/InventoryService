using Akka.Actor;
using Akka.Configuration;
using System;
using System.Configuration;
using InventoryService.Diagnostics;
using NLog;

namespace InventoryService.ActorSystemFactoryLib
{
    public class ActorSystemFactory
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Any actor system passed in can be terminated if 'TerminateActorSystem()' is called on a disposable
        /// </summary>
        /// <param name="serverActorSystemName"></param>
        /// <param name="actorSystem"></param>
        /// <param name="actorSystemConfig"></param>
        public  void CreateOrSetUpActorSystem(string serverActorSystemName = null, ActorSystem actorSystem = null, string actorSystemConfig = null)
        {
            var actorSystemName = "";
            actorSystemName = string.IsNullOrEmpty(serverActorSystemName) ? ConfigurationManager.AppSettings["ServerActorSystemName"] : serverActorSystemName;
            InventoryServiceDiagnostics.Debug(() =>
            {
                
    InventoryServiceActorSystem = string.IsNullOrEmpty(actorSystemName)
                ? actorSystem
                : (string.IsNullOrEmpty(actorSystemConfig)
                    ? Akka.Actor.ActorSystem.Create(actorSystemName)
                    : Akka.Actor.ActorSystem.Create(serverActorSystemName,actorSystemConfig));
            });
        

            if (InventoryServiceActorSystem != null) return;
            const string message = "Invalid ActorSystemName.Please set up 'ServerActorSystemName' in the config file";
           Log.Debug(message);
            throw new Exception(message);
        }

        public  ActorSystem InventoryServiceActorSystem { get; set; }

        public  void TerminateActorSystem()
        {
            //InventoryServiceActorSystem.Terminate();
            //InventoryServiceActorSystem.Dispose();
        }
    }
}