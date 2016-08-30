using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace InventoryService.Storage.InMemoryLib
{
    public class InMemory : IInventoryStorage
    {
        private readonly ConcurrentDictionary<string, RealTimeInventory> _productInventories =
            new ConcurrentDictionary<string, RealTimeInventory>();

        public async Task<StorageOperationResult<RealTimeInventory>> ReadInventory(string productId)
        {
            if (_productInventories.ContainsKey(productId))
            {
                return await Task.FromResult(new StorageOperationResult<RealTimeInventory>(_productInventories[productId]));
            }
            else
            {
                return await Task.FromResult(new StorageOperationResult<RealTimeInventory>(new RealTimeInventory(productId, 0, 0, 0)));
            }
        }

        public async Task<StorageOperationResult> WriteInventory(RealTimeInventory inventoryObject)
        {
            StorageWriteCheck.Execute(inventoryObject);

            _productInventories.AddOrUpdate(inventoryObject.ProductId, new RealTimeInventory(inventoryObject.ProductId, inventoryObject.Quantity, inventoryObject.Reservations, inventoryObject.Holds),
                (key, oldValue) => new RealTimeInventory(inventoryObject.ProductId, inventoryObject.Quantity, inventoryObject.Reservations, inventoryObject.Holds));
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