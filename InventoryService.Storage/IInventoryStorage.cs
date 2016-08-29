using System;
using System.Threading.Tasks;

namespace InventoryService.Storage
{
    public interface IInventoryStorage : IDisposable
    {
        Task<StorageOperationResult<RealTimeInventory>> ReadInventory(string productId);

        Task<StorageOperationResult> WriteInventory(RealTimeInventory inventoryObject);

        Task<bool> Flush(string productId);
    }
}