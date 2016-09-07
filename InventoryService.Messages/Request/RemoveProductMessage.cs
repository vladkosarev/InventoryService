using InventoryService.Messages.Models;
using System;

namespace InventoryService.Messages.Request
{
    public class RemoveProductMessage
    {
        public RemoveProductMessage(RealTimeInventory realTimeInventory, Exception reason = null)
        {
            RealTimeInventory = realTimeInventory;
            Reason = reason;
        }

        public RealTimeInventory RealTimeInventory { private set; get; }
        public Exception Reason { private set; get; }
    }
}