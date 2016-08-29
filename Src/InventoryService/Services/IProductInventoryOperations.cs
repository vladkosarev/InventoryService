using System.Threading.Tasks;
using InventoryService.Storage;

namespace InventoryService.Services
{
    public interface IProductInventoryOperations
    {
        Task<RealTimeInventory> ReadInventory(string productId);
        Task<bool> InventoryStorageFlush(string id);
        Task<bool> Reserve(string productId, int reservationQuantity);
        Task<bool> UpdateQuantity(string productId, int quantity);
        Task<bool> PlaceHold(string productId, int toHold);
        Task<bool> Purchase(string productId, int quantity);
        Task<bool> PurchaseFromHolds(string productId, int quantity);
    }
}