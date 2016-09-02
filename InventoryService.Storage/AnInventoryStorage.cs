using InventoryService.Messages.Models;
using System;
using System.Threading.Tasks;

namespace InventoryService.Storage
{
    public abstract class AnInventoryStorage : IDisposable
    {
        protected abstract Task<StorageOperationResult<IRealTimeInventory>> AReadInventoryAsync(string productId);

        protected abstract Task<StorageOperationResult> AWriteInventoryAsync(IRealTimeInventory inventoryObject);

        protected abstract Task<bool> AFlushAsync(string productId);

        public async Task<StorageOperationResult<IRealTimeInventory>> ReadInventoryAsync(string productId)
        {
            try
            {
                return await AReadInventoryAsync(productId);
            }
            catch (Exception)
            {
                
           return  new StorageOperationResult<IRealTimeInventory>(new RealTimeInventory(null,0,0,0));
            }
        }

        public async Task<StorageOperationResult> WriteInventoryAsync(IRealTimeInventory inventoryObject)
        {
            try
            {
                return await AWriteInventoryAsync(inventoryObject);
            }
            catch (Exception)
            {

                return new StorageOperationResult();
            }
        }

        public async Task<bool> FlushAsync(string productId)
        {
            try
            {
                return await AFlushAsync(productId);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void Dispose()
        {
            //todo
        }
    }
}