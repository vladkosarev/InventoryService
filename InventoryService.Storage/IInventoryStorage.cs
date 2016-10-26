using InventoryService.Messages.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryService.Storage
{
    public interface IInventoryStorage : IDisposable
    {
        Task<StorageOperationResult<List<string>>> ReadAllInventoryIdAsync();

        Task<StorageOperationResult<IRealTimeInventory>> ReadInventoryAsync(string productId);

        Task<StorageOperationResult> WriteInventoryAsync(IRealTimeInventory inventoryObject);

        Task<bool> FlushAsync();
    }
}