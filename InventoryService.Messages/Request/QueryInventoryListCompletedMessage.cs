using InventoryService.Messages.Models;
using System.Collections.Generic;

namespace InventoryService.Messages.Request
{
    public class RealTimeInventoryChangeMessage
    {
        public RealTimeInventoryChangeMessage(IRealTimeInventory realTimeInventory)
        {
            RealTimeInventory = realTimeInventory;
        }

        public IRealTimeInventory RealTimeInventory { get; }
    }

    public class QueryInventoryCompletedMessage
    {
        public QueryInventoryCompletedMessage(List<IRealTimeInventory> realTimeInventory, double speed, double peakMessageSpeed)
        {
            RealTimeInventories = realTimeInventory;
            Speed = speed;
            PeakMessageSpeed = peakMessageSpeed;
        }

        public List<IRealTimeInventory> RealTimeInventories { get; }
        public double Speed { get; }
        public double PeakMessageSpeed { get; }
    }
}