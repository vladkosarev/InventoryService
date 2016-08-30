using System;
using System.Threading.Tasks;
using InventoryService.Messages.Models;

namespace InventoryService.Storage
{
    public interface IInventoryStorage : IDisposable
    {
        Task<StorageOperationResult<IRealTimeInventory>> ReadInventory(string productId);

        Task<StorageOperationResult> WriteInventory(IRealTimeInventory inventoryObject);

        Task<bool> Flush(string productId);
    }
}