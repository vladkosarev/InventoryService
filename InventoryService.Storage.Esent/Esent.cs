using InventoryService.Messages.Models;
using Microsoft.Isam.Esent.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace InventoryService.Storage.EsentLib
{
    public class Esent : AnInventoryStorage
    {
        private readonly PersistentDictionary<string, Inventory> _data = new PersistentDictionary<string, Inventory>("InventoryStorageDBNew2");

        [Serializable]
        private struct Inventory
        {
            public Inventory(int quantity, int reservations, int holds)
            {
                Quantity = quantity;
                Reservations = reservations;
                Holds = holds;
            }

            public readonly int Quantity;
            public readonly int Reservations;
            public readonly int Holds;
        }

        protected override async Task<StorageOperationResult<IRealTimeInventory>> AReadInventoryAsync(string productId)
        {
            var value = _data.ContainsKey(productId) ? _data[productId] : new Inventory(0, 0, 0);
            return await Task.FromResult(new StorageOperationResult<IRealTimeInventory>(new RealTimeInventory(productId, value.Quantity, value.Reservations, value.Holds)));
        }

        protected override async Task<StorageOperationResult> AWriteInventoryAsync(IRealTimeInventory inventoryObject)
        {
            //StorageWriteCheck.Execute(inventoryObject);
            _data[inventoryObject.ProductId] = new Inventory(inventoryObject.Quantity, inventoryObject.Reserved, inventoryObject.Holds);
            return await Task.FromResult(new StorageOperationResult() { IsSuccessful = false });
        }

        protected override async Task<bool> AFlushAsync(string productId)
        {
            _data.Flush();
            return await Task.FromResult(true);
        }

        public void Dispose()
        {
            _data.Dispose();
        }
    }
}