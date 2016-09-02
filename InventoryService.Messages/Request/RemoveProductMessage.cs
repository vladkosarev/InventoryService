using System;
using InventoryService.Messages.Models;

namespace InventoryService.Messages.Request
{
    public class RemoveProductMessage
    {
        public RemoveProductMessage(RealTimeInventory realTimeInventory, Exception reason)
        {
            RealTimeInventory = realTimeInventory;
            Reason = reason;
        }

        public RealTimeInventory RealTimeInventory { private set; get; }
        public Exception Reason { private set; get; }
    }
}