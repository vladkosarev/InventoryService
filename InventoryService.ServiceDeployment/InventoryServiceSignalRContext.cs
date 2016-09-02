using Akka.Actor;
using InventoryService.Messages.Request;
using InventoryService.Server;
using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;

namespace InventoryService.ServiceDeployment
{
    public class InventoryServiceSignalRContext
    {
        public static void Push()
        {
            Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(c =>
            {
                var actor = InventoryServiceServer.ActorSystem.ActorSelection("akka.tcp://InventoryService-Server@localhost:8099/user/InventoryActor");
                actor.Ask<QueryInventoryListCompletedMessage>(new QueryInventoryListMessage())
                               .ContinueWith(
                                   x =>
                                   {
                                       GlobalHost.ConnectionManager.GetHubContext<InventoryServiceHub>().Clients.All.inventoryData(x.Result);
                                       Push();
                                   });
            });
        }
    }
}