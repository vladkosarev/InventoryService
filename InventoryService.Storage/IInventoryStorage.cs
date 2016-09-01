using InventoryService.Messages.Models;
using System;
using System.Threading.Tasks;

namespace InventoryService.Storage
{
    public interface IInventoryStorage : IDisposable
    {
        Task<StorageOperationResult<IRealTimeInventory>> ReadInventoryAsync(string productId);

        Task<StorageOperationResult> WriteInventoryAsync(IRealTimeInventory inventoryObject);

        Task<bool> FlushAsync(string productId);
    }
}