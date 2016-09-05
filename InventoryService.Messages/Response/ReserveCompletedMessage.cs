using InventoryService.Messages.Models;

namespace InventoryService.Messages.Response
{
    public class ReserveCompletedMessage : IInventoryServiceCompletedMessage
    {
        public ReserveCompletedMessage(IRealTimeInventory realTimeInventory, bool successful)
        {
            Successful = successful;

            RealTimeInventory = realTimeInventory;
        }

        public IRealTimeInventory RealTimeInventory { get; }

        public bool Successful { get; }
    }
}