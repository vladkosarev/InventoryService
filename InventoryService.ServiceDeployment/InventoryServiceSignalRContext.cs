using Akka.Actor;
using InventoryService.Messages.Request;
using InventoryService.Messages.Response;
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
                var product = "product";
                var actor = InventoryServiceServer.ActorSystem.ActorSelection("akka.tcp://InventoryService-Server@localhost:8099/user/InventoryActor");

                actor.Tell(new UpdateQuantityMessage(product, 1));

                actor.Ask<IInventoryServiceCompletedMessage>(new GetInventoryMessage(product))
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