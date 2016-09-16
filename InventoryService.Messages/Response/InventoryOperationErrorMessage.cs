using InventoryService.Messages.Models;

namespace InventoryService.Messages.Response
{
    public class InventoryOperationErrorMessage : IInventoryServiceCompletedMessage
    {
        public InventoryOperationErrorMessage(IRealTimeInventory realTimeInventory, RealTimeInventoryException error)
        {
            Error = error;
            RealTimeInventory = realTimeInventory;
            Successful = false;
        }

        public IRealTimeInventory RealTimeInventory { get; }
        public bool Successful { get; }
        public RealTimeInventoryException Error { get; private set; }
    }
}