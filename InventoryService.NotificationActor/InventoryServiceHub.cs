using Akka.Actor;
using InventoryService.ActorSystemFactoryLib;
using InventoryService.Messages.Request;
using Microsoft.AspNet.SignalR;
using System.Configuration;

namespace InventoryService.NotificationActor
{
    public class InventoryServiceHub : Hub
    {
        public void GetInventoryList()
        {
            var actor = GetRemoteActor();
            actor.Tell(new QueryInventoryListMessage());
        }

        private static ActorSelection GetRemoteActor()
        {
            var remoteAddress = ConfigurationManager.AppSettings["RemoteActorAddress"];
            var actor = ActorSystemFactory.InventoryServiceActorSystem.ActorSelection(remoteAddress);
            return actor;
        }
    }
}