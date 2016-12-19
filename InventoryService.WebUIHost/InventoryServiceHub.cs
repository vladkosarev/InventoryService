using Akka.Actor;
using InventoryService.Messages;
using Microsoft.AspNet.SignalR;
using NLog;

namespace InventoryService.WebUIHost
{
    public class InventoryServiceHub : Hub
    {
        private IActorRef SignalRInventoryQueryActorRef { set; get; }

        public InventoryServiceHub(IActorRef signalRInventoryQueryActorRef)
        {
            SignalRInventoryQueryActorRef = signalRInventoryQueryActorRef;
        }

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void GetInventoryList()
        {
            SignalRInventoryQueryActorRef.Tell(new GetAllInventoryListMessage());
            Log.Debug("A client has joined the system and will be receiving messages");
        }

        public void BackUpInventories()
        {
            Log.Debug("Exporting inventories");
            SignalRInventoryQueryActorRef.Tell(new ExportAllInventoryMessage());
        }

        public void PerformOperation(string operation, string id, int quantity, int retryCount)
        {
            SignalRInventoryQueryActorRef.Tell(new RequestInstructionIntoRemoteServermessage(operation, id, quantity, retryCount));
        }
    }
}