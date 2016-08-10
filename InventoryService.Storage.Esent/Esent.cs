using System;
using System.Threading.Tasks;
using Microsoft.Isam.Esent.Collections.Generic;

namespace InventoryService.Storage
{
    public class Esent : IInventoryStorage
    {
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

        private readonly PersistentDictionary<string, Inventory> _data = new PersistentDictionary<string, Inventory>("InventoryStorageDB");

        public async Task<Tuple<int, int, int>> ReadInventory(string productId)
        {
            var value = _data[productId];
            return new Tuple<int, int, int>(value.Quantity, value.Reservations, value.Holds);
        }

        public async Task<bool> WriteInventory(string productId, int quantity, int reservations, int holds)
        {
            _data[productId] = new Inventory(quantity, reservations, holds);
            return true;
        }

        public async Task Flush(string productId)
        {
            _data.Flush();
        }

        public void Dispose()
        {
            _data.Dispose();
        }
    }
}
