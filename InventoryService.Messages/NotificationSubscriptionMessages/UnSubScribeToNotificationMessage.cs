using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryService.Messages.NotificationSubscriptionMessages
{
    public class UnSubScribeToNotificationMessage
    {
        public UnSubScribeToNotificationMessage(Guid subscriptionId)
        {
            SubscriptionId = subscriptionId;
        }

        public Guid SubscriptionId { get; private set; }
    }
}
