using InventoryService.Messages.Models;
using InventoryService.Storage;
using System;
using System.Threading.Tasks;

namespace InventoryService.Services
{
    public static class ProductInventoryOperations 
    {
     
        public static  RealTimeInventory Initialize(IInventoryStorage inventoryStorage, string id)
        {
            
            var initTask = inventoryStorage.ReadInventory(id);
            Task.WaitAll(initTask);
            var inventory = initTask.Result.Result;

            if (!initTask.Result.IsSuccessful)
            {
                throw initTask.Result.Errors.Flatten();
            }
            return new RealTimeInventory(id, inventory.Quantity, inventory.Reserved, inventory.Holds);
        }

        private static OperationResult<IRealTimeInventory> _ReadInventory(this RealTimeInventory _realTimeInventory)
        {
            return new OperationResult<IRealTimeInventory>()
            {
                Data = _realTimeInventory
            };
        }

        public static async Task<OperationResult<IRealTimeInventory>> ReadInventory(this RealTimeInventory _realTimeInventory, IInventoryStorage _inventoryStorage, string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(productId)) return new ArgumentNullException(nameof(productId)).ToFailedOperationResult(productId);
                var result = await _inventoryStorage.ReadInventory(productId);
                return result.Result.ToSuccessOperationResult();
            }
            catch (Exception e)
            {
                return e.ToFailedOperationResult("Unable to read inventory for product " + productId);
            }
        }

        public static async Task<OperationResult<IRealTimeInventory>> InventoryStorageFlush(this RealTimeInventory _realTimeInventory, IInventoryStorage _inventoryStorage, string id)
        {
            await _inventoryStorage.Flush(id);
             return _ReadInventory(_realTimeInventory);
        }

        public static async Task<OperationResult<IRealTimeInventory>> Reserve(this RealTimeInventory _realTimeInventory, IInventoryStorage _inventoryStorage, string productId, int reservationQuantity)
        {
            var newReserved = Math.Max(0, _realTimeInventory.Reserved + reservationQuantity);

            if (newReserved > _realTimeInventory.Quantity - _realTimeInventory.Holds)
                return new Exception("What to reserve must be less than quantity - reservation for product " +
                                     productId).ToFailedOperationResult(productId);

            var result =
                await
                    _inventoryStorage.WriteInventory(new RealTimeInventory(productId, _realTimeInventory.Quantity, newReserved, _realTimeInventory.Holds));

            if (!result.IsSuccessful)
                return new Exception("Could not load inventory for product " + productId, result.Errors).ToFailedOperationResult(productId);
          

            return _ReadInventory(_realTimeInventory);
        }

        public static async Task<OperationResult<IRealTimeInventory>> UpdateQuantity(this RealTimeInventory _realTimeInventory, IInventoryStorage _inventoryStorage, string productId, int quantity)
        {
            var newQuantity = _realTimeInventory.Quantity + quantity;

            var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, _realTimeInventory.Reserved, _realTimeInventory.Holds));

            if (!result.IsSuccessful) return result.Errors.Flatten().ToFailedOperationResult(productId);
            _realTimeInventory = result.Result as RealTimeInventory;

            return _ReadInventory(_realTimeInventory);
        }

        public static async Task<OperationResult<IRealTimeInventory>> UpdateQuantityAndHold(this RealTimeInventory _realTimeInventory, IInventoryStorage _inventoryStorage, string productId, int quantity)
        {
            var newQuantity = _realTimeInventory.Quantity + quantity;
            var newHolds = _realTimeInventory.Holds + quantity;
            if (newHolds > _realTimeInventory.Quantity)
            {
                return new Exception("Unable to update quantity and hold With the supplied hold " + quantity + ",the new hold is larger than resulting quantity for product " + productId).ToFailedOperationResult(productId);
            }

            var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, _realTimeInventory.Reserved, _realTimeInventory.Holds));

            if (!result.IsSuccessful) return result.Errors.Flatten().ToFailedOperationResult(productId);
            _realTimeInventory = result.Result as RealTimeInventory;

             return _ReadInventory(_realTimeInventory);
        }

        public static async Task<OperationResult<IRealTimeInventory>> PlaceHold(this RealTimeInventory _realTimeInventory, IInventoryStorage _inventoryStorage, string productId, int toHold)
        {
            var newHolds = _realTimeInventory.Holds + toHold;
            if (newHolds > _realTimeInventory.Quantity)
            {
                return new Exception("With the supplied hold " + toHold + ",the new hold is larger than available quantity for product " + productId).ToFailedOperationResult(productId);
            }
            var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, _realTimeInventory.Quantity, _realTimeInventory.Reserved, newHolds));

            if (!result.IsSuccessful) return result.Errors.Flatten().ToFailedOperationResult(productId);
            _realTimeInventory = result.Result as RealTimeInventory;
             return _ReadInventory(_realTimeInventory);
        }

        public static async Task<OperationResult<IRealTimeInventory>> Purchase(this RealTimeInventory _realTimeInventory, IInventoryStorage _inventoryStorage, string productId, int quantity)
        {
            if (quantity < 0)
            {
                return new Exception("Cannot purchase with a negative quantity of " + quantity + " for product " + productId).ToFailedOperationResult(productId);
            }
            var newQuantity = _realTimeInventory.Quantity - quantity;

            var newReserved = Math.Max(0, _realTimeInventory.Reserved - quantity);
            //todo we still want o sell even though there is reservation
            //var newReserved = _realTimeInventory.Reserved - quantity;
            //if(newReserved<0) throw new Exception("provided " + quantity + ", available reservations must be less than or equal to quantity for product " + productId);

            if (newQuantity - _realTimeInventory.Holds < 0) return new Exception("provided " + quantity + ", available holds must be less than or equal to quantity for product " + productId).ToFailedOperationResult(productId);
            var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, newReserved, _realTimeInventory.Holds));

            if (!result.IsSuccessful) return result.Errors.Flatten().ToFailedOperationResult(productId);
            _realTimeInventory = result.Result as RealTimeInventory;

             return _ReadInventory(_realTimeInventory);
        }

        public static async Task<OperationResult<IRealTimeInventory>> PurchaseFromHolds(this RealTimeInventory _realTimeInventory, IInventoryStorage _inventoryStorage, string productId, int quantity)
        {
            if (quantity < 0)
            {
                return new Exception("Cannot purchase from holds with a negative quantity of " + quantity + " for product " + productId).ToFailedOperationResult(productId);
            }
            var newQuantity = _realTimeInventory.Quantity - quantity;
            var newHolds = _realTimeInventory.Holds - quantity;

            if (newQuantity < 0 || newHolds < 0) return new Exception("Purchase from negative hold or a negative purchase is not allowed  for product " + productId).ToFailedOperationResult(productId);
            var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, _realTimeInventory.Reserved, newHolds)).ConfigureAwait(false);

            if (!result.IsSuccessful) return result.Errors.Flatten().ToFailedOperationResult(productId);
            _realTimeInventory = result.Result as RealTimeInventory;
             return _ReadInventory(_realTimeInventory);
        }

    
    }
}