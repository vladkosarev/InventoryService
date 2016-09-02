using System.Collections.Generic;
using InventoryService.Messages.Models;

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