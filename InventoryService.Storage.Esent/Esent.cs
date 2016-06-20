using System;
using System.Threading.Tasks;
using Microsoft.Isam.Esent.Collections.Generic;

namespace InventoryService.Storage
{
    public class Esent : IInventoryStorage
    {
        [Serializable]
        struct Inventory
        {
            public Inventory(int quantity, int reservations)
            {
                Quantity = quantity;
                Reservations = reservations;
            }

            public readonly int Quantity;
            public readonly int Reservations;
        }

        private readonly PersistentDictionary<string, Inventory> _data = new PersistentDictionary<string, Inventory>("esent");

        public async Task<Tuple<int, int>> ReadInventory(string productId)
        {
            var value = _data[productId];
            return new Tuple<int, int>(value.Quantity, value.Reservations);
        }

        public async Task<bool> WriteInventory(string productId, int quantity, int reservationQuantity)
        {
            _data[productId] = new Inventory(quantity, reservationQuantity);
            return true;
        }

        public async Task Flush(string productId)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _data.Dispose();
        }
    }
}
