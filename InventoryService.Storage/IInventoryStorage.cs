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

    /*
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
                return false;
            }
        }

        public void Dispose()
        {
            //todo how to dispose
        }
    }
     */
}