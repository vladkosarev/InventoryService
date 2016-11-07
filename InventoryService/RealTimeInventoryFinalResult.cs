using InventoryService.Messages;
using InventoryService.Messages.Models;

namespace InventoryService
{
    public class RealTimeInventoryFinalResult
    {
        public RealTimeInventoryFinalResult(RealTimeInventory realTimeInventory, IInventoryServiceCompletedMessage inventoryServiceCompletedMessage, OperationResult<IRealTimeInventory> result)
        {
            RealTimeInventory = realTimeInventory;
            InventoryServiceCompletedMessage = inventoryServiceCompletedMessage;
            Result = result;
        }

        public RealTimeInventory RealTimeInventory { get; }
        public IInventoryServiceCompletedMessage InventoryServiceCompletedMessage { get; }
        public OperationResult<IRealTimeInventory> Result { get; }
    }
}