using System;
using System.Threading.Tasks;
using Microsoft.Isam.Esent.Collections.Generic;

namespace InventoryService.Storage.Esent
{
    public class Esent : IInventoryStorage, IDisposable
    {
        [Serializable]
        struct Inventory
        {
            public Inventory(int quantity, int reservations)
            {
                Quantity = quantity;
                Reservations = reservations;
            }

            public int Quantity;
            public int Reservations;
        }

        PersistentDictionary<string, Inventory> dictionary = new PersistentDictionary<string, Inventory>("esent");

        public async Task<Tuple<int, int>> ReadInventory(string productId)
        {
            var value = dictionary[productId];
            return new Tuple<int, int>(value.Quantity, value.Reservations);
        }

        public async Task<bool> WriteInventory(string productId, int quantity, int reservationQuantity)
        {
            dictionary[productId] = new Inventory(quantity, reservationQuantity);
            return true;
        }

        public async Task Flush(string productId)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            dictionary.Dispose();
        }
    }
}
