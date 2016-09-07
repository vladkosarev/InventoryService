using InventoryService.Messages.Models;

namespace InventoryService.Messages
{
    public interface IInventoryServiceCompletedMessage
    {
        IRealTimeInventory RealTimeInventory { get; }
        bool Successful { get; }
    }
}