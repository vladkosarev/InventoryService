using InventoryService.Messages.Models;
using System.Threading.Tasks;

namespace InventoryService
{
    public interface IProductInventoryOperations
    {
        Task<OperationResult<IRealTimeInventory>> ReadInventory(string productId);

        Task<OperationResult<IRealTimeInventory>> InventoryStorageFlush(string id);

        Task<OperationResult<IRealTimeInventory>> Reserve(string productId, int reservationQuantity);

        Task<OperationResult<IRealTimeInventory>> UpdateQuantity(string productId, int quantity);

        Task<OperationResult<IRealTimeInventory>> UpdateQuantityAndHold(string productId, int quantity);

        Task<OperationResult<IRealTimeInventory>> PlaceHold(string productId, int toHold);

        Task<OperationResult<IRealTimeInventory>> Purchase(string productId, int quantity);

        Task<OperationResult<IRealTimeInventory>> PurchaseFromHolds(string productId, int quantity);
    }
}