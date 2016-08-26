﻿using System;
using System.Threading.Tasks;
using Microsoft.Isam.Esent.Collections.Generic;

namespace InventoryService.Storage.EsentLib
{
    public class Esent : IInventoryStorage
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


        public async Task<RealTimeInventory> ReadInventory(string productId)
        {
            var value = _data.ContainsKey(productId) ? _data[productId] : new Inventory(0, 0, 0);
            return await Task.FromResult(new RealTimeInventory(productId, value.Quantity, value.Reservations, value.Holds));
        }

        public async Task<bool> WriteInventory(RealTimeInventory inventoryObject)
        {
            _data[inventoryObject.ProductId] = new Inventory(inventoryObject.Quantity, inventoryObject.Reservations, inventoryObject.Holds);
            return await Task.FromResult(true);
        }

        public async Task<bool> Flush(string productId)
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