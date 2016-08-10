using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryService.Storage
{
    public class InMemory : IInventoryStorage
    {
        private readonly ConcurrentDictionary<string, Tuple<int, int, int>> _productInventories =
            new ConcurrentDictionary<string, Tuple<int, int, int>>();

        public async Task<Tuple<int, int, int>> ReadInventory(string productId)
        {
            return _productInventories[productId];
        }

        public async Task<bool> WriteInventory(string productId, int quantity, int reservations, int holds)
        {
            _productInventories.AddOrUpdate(productId, new Tuple<int, int, int>(quantity, reservations, holds),
                (key, oldValue) => new Tuple<int, int, int>(quantity, reservations, holds));
            return true;
        }

        public async Task Flush()
        {
        }

        public Task Flush(string productId)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}
