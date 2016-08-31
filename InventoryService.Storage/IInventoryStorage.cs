using InventoryService.Messages.Models;
using System;
using System.Threading.Tasks;

namespace InventoryService.Storage
{
    public interface IInventoryStorage : IDisposable
    {
        Task<StorageOperationResult<IRealTimeInventory>> ReadInventory(string productId);

        Task<StorageOperationResult> WriteInventory(IRealTimeInventory inventoryObject);

        Task<bool> Flush(string productId);
    }
}