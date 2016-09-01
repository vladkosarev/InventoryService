using InventoryService.Messages.Models;

namespace InventoryService.Messages.Response
{
    public interface IInventoryServiceCompletedMessage
    {
      
         IRealTimeInventory RealTimeInventory { get; }
        bool Successful { get; }
    }
}