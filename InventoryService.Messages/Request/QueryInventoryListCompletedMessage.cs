using InventoryService.Messages.Models;
using System.Collections.Generic;

namespace InventoryService.Messages.Request
{
    public class QueryInventoryListCompletedMessage
    {
        public QueryInventoryListCompletedMessage(List<IRealTimeInventory> realTimeInventories)
        {
            RealTimeInventories = realTimeInventories;
        }

        public List<IRealTimeInventory> RealTimeInventories { get; private set; }
    }
}