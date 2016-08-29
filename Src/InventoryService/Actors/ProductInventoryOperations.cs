using System;
using System.Threading.Tasks;
using InventoryService.Storage;

namespace InventoryService.Actors
{
    public class ProductInventoryOperations : IProductInventoryOperations
    {

        private int _quantity;
        private int _reservations;
        private int _holds;
        private readonly IInventoryStorage _inventoryStorage;
        public ProductInventoryOperations(IInventoryStorage inventoryStorage, string id)
        {
            _inventoryStorage = inventoryStorage;
            var initTask = _inventoryStorage.ReadInventory(id);
            Task.WaitAll(initTask);
            var inventory = initTask.Result;
            _quantity = inventory.Quantity;
            _reservations = inventory.Reservations;
            _holds = inventory.Holds;

        }


        public async Task<RealTimeInventory> ReadInventory(string productId)
        {
            return await _inventoryStorage.ReadInventory(productId);
        }
        public async Task<bool> InventoryStorageFlush(string id)
        {
          await  _inventoryStorage.Flush(id);
            return true;
        }

        public async Task<bool> Reserve(string productId, int reservationQuantity)
        {
            var newReserved = _reservations + reservationQuantity;
            if (newReserved > _quantity - _holds) return false;
            var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, _quantity, newReserved, _holds));

            if (!result) return false;
            _reservations = newReserved;
            return true;
        }

        public async Task<bool> UpdateQuantity(string productId, int quantity)
        {
            var newQuantity = _quantity + quantity;

            var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, _reservations, _holds));

            if (!result) return false;
            _quantity = newQuantity;
            return true;
        }

        public async Task<bool> PlaceHold(string productId, int toHold)
        {
            var newHolds = _holds + toHold;
            if (newHolds > _quantity) return false;
            var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, _quantity, _reservations, newHolds));

            if (!result) return false;
            _holds = newHolds;
            return true;
        }

        public async Task<bool> Purchase(string productId, int quantity)
        {
            var newQuantity = _quantity - quantity;
            var newReserved = Math.Max(0, _reservations - quantity);

            if (newQuantity - _holds < 0) return false;
            var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, newReserved, _holds));

            if (!result) return false;
            _quantity = newQuantity;
            _reservations = newReserved;
            return true;
        }

        public async Task<bool> PurchaseFromHolds(string productId, int quantity)
        {
            var newQuantity = _quantity - quantity;
            var newHolds = _holds - quantity;

            if (newQuantity < 0 || newHolds < 0) return false;
            var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, _reservations, newHolds));

            if (!result) return false;
            _quantity = newQuantity;
            _holds = newHolds;
            return true;
        }
    }
}