using InventoryService.Messages.Models;

namespace InventoryService.Messages.Response
{
    public class GetInventoryCompletedMessage : IInventoryServiceCompletedMessage
    {
        public IRealTimeInventory RealTimeInventory { get; }

        public bool Successful { get; }

        public GetInventoryCompletedMessage(IRealTimeInventory realTimeInventory, bool successful)
        {
            Successful = successful;

            RealTimeInventory = realTimeInventory;
        }
    }
}