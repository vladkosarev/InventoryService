using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace InventoryService.Storage
{
    public class InMemory : IInventoryStorage
    {
        private readonly ConcurrentDictionary<string, Tuple<int, int>> _productInventories =
            new ConcurrentDictionary<string, Tuple<int, int>>();

        public async Task<Tuple<int, int>> ReadInventory(string productId)
        {
            return _productInventories[productId];
        }

        public async Task<bool> WriteInventory(string productId, int quantity, int reservationQuantity)
        {
            _productInventories.AddOrUpdate(productId, new Tuple<int, int>(quantity, reservationQuantity),
                (key, oldValue) => new Tuple<int, int>(quantity, reservationQuantity));
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
