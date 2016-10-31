using Akka.Actor;
using InventoryService.ActorSystemFactoryLib;
using InventoryService.Messages.Request;
using Microsoft.AspNet.SignalR;
using System.Configuration;
using NLog;

namespace InventoryService.NotificationActor
{
    public class InventoryServiceHub : Hub
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private ActorSystem HubActorSystem { set; get; }

        public void GetInventoryList()
        {
            var actor = GetRemoteActor();
            actor.Tell(new QueryInventoryListMessage());
        }

        private ActorSelection GetRemoteActor()
        {
            if (HubActorSystem == null)
            {
                var name = "SignalRHubActorSystem";
                Log.Debug("Creating actor system :" + name);
                HubActorSystem = ActorSystem.Create(name, @"
                   akka {
                   loggers = [""Akka.Logger.NLog.NLogLogger,Akka.Logger.NLog""]
                   stdout-loglevel = DEBUG
                   loglevel = DEBUG
                   log-config-on-start = on
                    }
                   akka.actor{                     
                      debug {
			                receive = on
				            autoreceive = on
				            lifecycle = on
				            event-stream = on
				            unhandled = on
		            }
                   }
              ");
            }

            RemoteAddress = RemoteAddress ?? ConfigurationManager.AppSettings["RemoteActorAddress"];
            Log.Debug("Notification hub trying to locate remote actor at " + RemoteAddress);
            var actor = HubActorSystem.ActorSelection(RemoteAddress);
            return actor;
        }

        public string RemoteAddress { get; set; }
    }
}