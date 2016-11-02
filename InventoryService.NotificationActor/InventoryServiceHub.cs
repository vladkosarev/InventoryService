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
        private static ActorSystem HubActorSystem { set; get; }

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
                Log.Debug("Trying to create notification hub actor system :" + name);
                HubActorSystem = HubActorSystem?? ActorSystem.Create(name, @"
                   akka {
                   #loggers = [""Akka.Logger.NLog.NLogLogger,Akka.Logger.NLog""]
                   #stdout-loglevel = DEBUG
                   #loglevel = DEBUG
                   #log-config-on-start = on
                    }
                   akka.actor{
                      provider= ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
                      debug {
			                #receive = on
				            #autoreceive = on
				            #lifecycle = on
				            #event-stream = on
				            #unhandled = on
		            }
                   }
                  akka.remote {
                      #log-remote-lifecycle-events = DEBUG
                      #log-received-messages = on
                      #log-sent-messages = on
                    helios.tcp {
                      #log-remote-lifecycle-events = DEBUG
                      #log-received-messages = on
                      #log-sent-messages = on
                      transport-class =""Akka.Remote.Transport.Helios.HeliosTcpTransport, Akka.Remote""
                      port = 0
                      transport-protocol = tcp
                      hostname = ""localhost""
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