using InventoryService.Messages.Models;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace InventoryService.Storage.InMemoryLib
{
    public class InMemory : AnInventoryStorage
    {
        private readonly ConcurrentDictionary<string, IRealTimeInventory> _productInventories =
            new ConcurrentDictionary<string, IRealTimeInventory>();

        protected override async Task<StorageOperationResult<IRealTimeInventory>> AReadInventoryAsync(string productId)
        {
            if (_productInventories.ContainsKey(productId))
            {
                return await Task.FromResult(new StorageOperationResult<IRealTimeInventory>(_productInventories[productId]));
            }
            else
            {
                return await Task.FromResult(new StorageOperationResult<IRealTimeInventory>(new RealTimeInventory(productId, 0, 0, 0)));
            }
        }

        protected override async Task<StorageOperationResult> AWriteInventoryAsync(IRealTimeInventory inventoryObject)
        {
            _productInventories.AddOrUpdate(inventoryObject.ProductId, new RealTimeInventory(inventoryObject.ProductId, inventoryObject.Quantity, inventoryObject.Reserved, inventoryObject.Holds),
                (key, oldValue) => new RealTimeInventory(inventoryObject.ProductId, inventoryObject.Quantity, inventoryObject.Reserved, inventoryObject.Holds));
            return await Task.FromResult(new StorageOperationResult() { IsSuccessful = true });
        }

        protected  override async Task<bool> AFlushAsync(string productId)
        {
            return await Task.FromResult(true);
        }

       
    }
}