using InventoryService.Messages.Models;

namespace InventoryService.Messages.Response
{
    public class UpdateQuantityCompletedMessage : IInventoryServiceCompletedMessage
    {
        public UpdateQuantityCompletedMessage(IRealTimeInventory realTimeInventory, bool successful)
        {
            Successful = successful;

            RealTimeInventory = realTimeInventory;
        }

        public IRealTimeInventory RealTimeInventory { get; }

        public bool Successful { get; }
    }
}