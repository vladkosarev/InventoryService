using InventoryService.Messages.Models;
using System.Collections.Generic;

namespace InventoryService.Messages.Request
{
    public class QueryInventoryListCompletedMessage
    {
        public QueryInventoryListCompletedMessage(List<RealTimeInventory> realTimeInventories)
        {
            RealTimeInventories = realTimeInventories;
        }

        public List<RealTimeInventory> RealTimeInventories { get; private set; }
    }
}