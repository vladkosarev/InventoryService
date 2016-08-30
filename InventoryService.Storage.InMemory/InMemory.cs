using System.Collections.Concurrent;
using System.Threading.Tasks;
using InventoryService.Messages.Models;

namespace InventoryService.Storage.InMemoryLib
{
    public class InMemory : IInventoryStorage
    {
        private readonly ConcurrentDictionary<string, IRealTimeInventory> _productInventories =
            new ConcurrentDictionary<string, IRealTimeInventory>();

        public async Task<StorageOperationResult<IRealTimeInventory>> ReadInventory(string productId)
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

        public async Task<StorageOperationResult> WriteInventory(IRealTimeInventory inventoryObject)
        {
            StorageWriteCheck.Execute(inventoryObject);

            _productInventories.AddOrUpdate(inventoryObject.ProductId, new RealTimeInventory(inventoryObject.ProductId, inventoryObject.Quantity, inventoryObject.Reserved, inventoryObject.Holds),
                (key, oldValue) => new RealTimeInventory(inventoryObject.ProductId, inventoryObject.Quantity, inventoryObject.Reserved, inventoryObject.Holds));
            return await Task.FromResult(new StorageOperationResult() { IsSuccessful = true });
        }

        public async Task<bool> Flush(string productId)
        {
            return await Task.FromResult(true);
        }

        public void Dispose()
        {
        }
    }
}