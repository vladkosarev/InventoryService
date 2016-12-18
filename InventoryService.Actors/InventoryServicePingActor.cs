using Akka.Actor;
using InventoryService.Messages;

namespace InventoryService.Actors
{
    public class InventoryServicePingActor : ReceiveActor
    {
        public InventoryServicePingActor()
        {
            Receive<InventoryServicePingMessage>(_ => Sender.Tell(new InventoryServicePongMessage()));
        }
    }
}