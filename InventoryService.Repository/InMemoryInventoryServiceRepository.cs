using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace InventoryService.Repository
{
    public class InMemoryInventoryServiceRepository : IInventoryServiceRepository
    {
        private readonly ConcurrentDictionary<string, Tuple<int, int>> _productInventories =
            new ConcurrentDictionary<string, Tuple<int, int>>();

        public async Task<int> ReadQuantity(string productId)
        {
            if (!_productInventories.ContainsKey(productId))
                throw new InvalidOperationException();
            return _productInventories[productId].Item1;
        }

        public async Task<int> ReadReservations(string productId)
        {
            if (!_productInventories.ContainsKey(productId))
                throw new InvalidOperationException();
            return _productInventories[productId].Item2;
        }

        public async Task<Tuple<int, int>> ReadQuantityAndReservations(string productId)
        {
            if (!_productInventories.ContainsKey(productId))
                throw new InvalidOperationException();
            return _productInventories[productId];
        }

        public async Task<bool> WriteQuantity(string productId, int quantity)
        {
            if (!_productInventories.ContainsKey(productId))
                _productInventories.TryAdd(productId, new Tuple<int, int>(quantity, 0));
            else
                _productInventories[productId] = new Tuple<int, int>(quantity, _productInventories[productId].Item2);
            return true;
        }

        public async Task<bool> WriteReservations(string productId, int reservationQuantity)
        {
            if (!_productInventories.ContainsKey(productId))
                _productInventories.TryAdd(productId, new Tuple<int, int>(0, reservationQuantity));
            else
                _productInventories[productId] = new Tuple<int, int>(_productInventories[productId].Item1,
                    reservationQuantity);
            return true;
        }

        public async Task<bool> WriteQuantityAndReservations(string productId, int quantity, int reservationQuantity)
        {
            if (!_productInventories.ContainsKey(productId))
                _productInventories.TryAdd(productId, new Tuple<int, int>(quantity, reservationQuantity));
            else
                _productInventories[productId] = new Tuple<int, int>(quantity, reservationQuantity);
            return true;
        }

        public async Task Flush()
        {
        }

        public Task Flush(string productId)
        {
            throw new NotImplementedException();
        }
    }
}
