using InventoryService.Messages.Models;
using Microsoft.Isam.Esent.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace InventoryService.Storage.EsentLib
{
    public class Esent : IInventoryStorage
    {
        public Esent(string storageName = "InventoryStorageDB")
        {
            if (string.IsNullOrEmpty(storageName)) throw new Exception(nameof(storageName) + " cannot be null or empty");
            Data = new PersistentDictionary<string, Inventory>(storageName);
        }

        private PersistentDictionary<string, Inventory> Data { set; get; }

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

        public async Task<StorageOperationResult<IRealTimeInventory>> ReadInventoryAsync(string productId)
        {
            var value = Data.ContainsKey(productId) ? Data[productId] : new Inventory(0, 0, 0);
            return await Task.FromResult(new StorageOperationResult<IRealTimeInventory>(new RealTimeInventory(productId, value.Quantity, value.Reservations, value.Holds)));
        }

        public async Task<StorageOperationResult> WriteInventoryAsync(IRealTimeInventory inventoryObject)
        {
            Data[inventoryObject.ProductId] = new Inventory(inventoryObject.Quantity, inventoryObject.Reserved, inventoryObject.Holds);
            return await Task.FromResult(new StorageOperationResult() { IsSuccessful = true });
        }

        public async Task<bool> FlushAsync()
        {
            Data.Flush();
            return await Task.FromResult(true);
        }

        public void Dispose()
        {
            Data.Dispose();
        }
    }
}