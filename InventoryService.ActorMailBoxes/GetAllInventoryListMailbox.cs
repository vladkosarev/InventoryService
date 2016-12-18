using Akka.Actor;
using Akka.Configuration;
using Akka.Dispatch;

namespace InventoryService.ActorMailBoxes
{
    public class GetAllInventoryListMailbox : UnboundedPriorityMailbox
    {
        protected override int PriorityGenerator(object message)
        {
            //return message is GetAllInventoryListMessage
            //    || message is CheckIfNotificationSubscriptionExistsMessage
            //    || message is SubScribeToNotificationMessage ? 0 : 1;

            return 1;
        }

        public GetAllInventoryListMailbox(Settings settings, Config config) : base(settings, config)
        {
        }
    }
}