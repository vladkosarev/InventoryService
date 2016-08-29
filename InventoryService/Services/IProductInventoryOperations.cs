using InventoryService.Storage;
using System.Threading.Tasks;

namespace InventoryService.Services
{
    public interface IProductInventoryOperations
    {
        Task<OperationResult<RealTimeInventory>> ReadInventory(string productId);

        Task<OperationResult<RealTimeInventory>> InventoryStorageFlush(string id);

        Task<OperationResult<RealTimeInventory>> Reserve(string productId, int reservationQuantity);

        Task<OperationResult<RealTimeInventory>> UpdateQuantity(string productId, int quantity);

        Task<OperationResult<RealTimeInventory>> PlaceHold(string productId, int toHold);

        Task<OperationResult<RealTimeInventory>> Purchase(string productId, int quantity);

        Task<OperationResult<RealTimeInventory>> PurchaseFromHolds(string productId, int quantity);
    }
}