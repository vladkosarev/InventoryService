using InventoryService.Messages.Models;

namespace InventoryService.Messages.Response
{
    public class ResetInventoryQuantityReserveAndHoldCompletedMessage : IInventoryServiceCompletedMessage
    {
        public ResetInventoryQuantityReserveAndHoldCompletedMessage(IRealTimeInventory realTimeInventory, bool successful)
        {
            RealTimeInventory = realTimeInventory;
            Successful = successful;
        }

        public IRealTimeInventory RealTimeInventory { get; }
        public bool Successful { get; }
    }
}