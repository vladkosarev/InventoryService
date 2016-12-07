using InventoryService.Messages.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InventoryService.Storage.InMemoryLib
{
    public class InMemory : IInventoryStorage
    {
        private  ConcurrentDictionary<string, IRealTimeInventory> _productInventories =
            new ConcurrentDictionary<string, IRealTimeInventory>();

        public Task<StorageOperationResult<IRealTimeInventory>> ReadInventoryAsync(string productId)
        {
            if (_productInventories.ContainsKey(productId))
            {
                return Task.FromResult(new StorageOperationResult<IRealTimeInventory>(_productInventories[productId]));
            }
            else
            {
                return Task.FromResult(new StorageOperationResult<IRealTimeInventory>(new RealTimeInventory(productId, 0, 0, 0)));
            }
        }

        public Task<StorageOperationResult<List<string>>> ReadAllInventoryIdAsync()
        {
            return Task.FromResult(new StorageOperationResult<List<string>>(_productInventories.Select(x => x.Key).ToList()));
        }

        public Task<StorageOperationResult> WriteInventoryAsync(IRealTimeInventory inventoryObject)
        {
            _productInventories.AddOrUpdate(inventoryObject.ProductId, new RealTimeInventory(inventoryObject.ProductId, inventoryObject.Quantity, inventoryObject.Reserved, inventoryObject.Holds),
                (key, oldValue) => new RealTimeInventory(inventoryObject.ProductId, inventoryObject.Quantity, inventoryObject.Reserved, inventoryObject.Holds));
            return Task.FromResult(new StorageOperationResult() { IsSuccessful = true });
        }

        public Task<bool> FlushAsync()
        {
            _productInventories=new ConcurrentDictionary<string, IRealTimeInventory>();
            return Task.FromResult(true);
        }

        public void Dispose()
        {
        }
    }
}