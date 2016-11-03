using Akka.Actor;
using InventoryService.Messages.Request;
using InventoryService.Server;
using Microsoft.AspNet.SignalR;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace InventoryService.WebUIDeployment
{
    public class InventoryServiceSignalRContext
    {
        public static void Push()
        {
            Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(c =>
            {
                GetInventoryData();
            });
        }

        public static void GetInventoryData()
        {
            var remoteAddress = ConfigurationManager.AppSettings["RemoteActorAddress"];
            var actor =InventoryServiceServer.ActorSystem.ActorSelection(remoteAddress);
            actor.Ask<QueryInventoryListCompletedMessage>(new QueryInventoryListMessage()) .ContinueWith(x =>
                    {
                        GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.inventoryData(x.Result);
                        Push();
                    });
        }
    }
}