using InventoryService.Messages.Models;

namespace InventoryService.Messages.Response
{
    public class UpdateAndHoldQuantityCompletedMessage : IInventoryServiceCompletedMessage
    {
        public UpdateAndHoldQuantityCompletedMessage(IRealTimeInventory realTimeInventory, bool successful)
        {
            RealTimeInventory = realTimeInventory;
            Successful = successful;
        }

        public IRealTimeInventory RealTimeInventory { get; }
        public bool Successful { get; }
    }
}