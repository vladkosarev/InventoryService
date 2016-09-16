using InventoryService.Messages;
using InventoryService.Messages.Models;
using System.Threading.Tasks;

namespace InventoryService.InMemory
{
    public interface IInventoryServiceDirect
    {
        Task<IInventoryServiceCompletedMessage> PurchaseFromHoldsAsync(RealTimeInventory RealTimeInventory, int update);

        Task<IInventoryServiceCompletedMessage> PurchaseAsync(RealTimeInventory RealTimeInventory, int update);

        Task<IInventoryServiceCompletedMessage> PlaceHoldAsync(RealTimeInventory RealTimeInventory, int update);

        Task<IInventoryServiceCompletedMessage> UpdateQuantityAndHoldAsync(RealTimeInventory RealTimeInventory, int update);

        Task<IInventoryServiceCompletedMessage> UpdateQuantityAsync(RealTimeInventory RealTimeInventory, int update);

        Task<IInventoryServiceCompletedMessage> ReserveAsync(RealTimeInventory RealTimeInventory, int update);

        Task<IInventoryServiceCompletedMessage> GetInventoryAsync(string productId);
    }
}