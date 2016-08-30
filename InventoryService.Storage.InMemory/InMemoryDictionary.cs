using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryService.Messages.Models;

namespace InventoryService.Storage.InMemoryLib
{
    public class InMemoryDictionary : IInventoryStorage
    {
        private readonly Dictionary<string, IRealTimeInventory> _productInventories = new Dictionary<string, IRealTimeInventory>();

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

            _productInventories[inventoryObject.ProductId] = new RealTimeInventory(inventoryObject.ProductId,
                inventoryObject.Quantity, inventoryObject.Reserved, inventoryObject.Holds);

            //  (key, oldValue) => new RealTimeInventory(inventoryObject.ProductId, inventoryObject.Quantity, inventoryObject.Reservations, inventoryObject.Holds));
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