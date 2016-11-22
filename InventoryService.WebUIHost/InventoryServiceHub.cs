using Akka.Actor;
using InventoryService.Messages;
using InventoryService.Messages.Request;
using Microsoft.AspNet.SignalR;
using NLog;

namespace InventoryService.WebUIHost
{
    public class InventoryServiceHub : Hub
    {
        private IActorRef SignalRNotificationsActorRef { set; get; }

        public InventoryServiceHub(IActorRef signalRNotificationsActorRef)
        {
            SignalRNotificationsActorRef = signalRNotificationsActorRef;
        }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void GetInventoryList()
        {
            SignalRNotificationsActorRef.Tell(new GetAllInventoryListMessage());
            Log.Debug("A client has joined the system and will be receiving messages");
        }
        
        public void PerformOperation(string operation,string id, int quantity,int numberOfTimes)
        {
            SignalRNotificationsActorRef.Tell(new RequestInstructionIntoRemoteServermessage(operation,id,quantity, numberOfTimes));
        }
    }
}