using InventoryService.Storage;
using System;
using System.Threading.Tasks;

namespace InventoryService.Services
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
            var inventory = initTask.Result.Result;

            if(!initTask.Result.IsSuccessful)
            {
                throw initTask.Result.Errors.Flatten();
            }
            _quantity = inventory.Quantity;
            _reservations = inventory.Reservations;
            _holds = inventory.Holds;
        }


        private string GetCurrentQuantitiesReport()
        {
            return " [ quantity : " + _quantity + " / reservations: " + _reservations + " / holds: " + _holds+" ]";
        }

        public async Task<OperationResult<RealTimeInventory>> ReadInventory(string productId)
        {
            try
            {
                var result = await _inventoryStorage.ReadInventory(productId);
                return result.Result.ToSuccessOperationResult();
            }
            catch (Exception e)
            {
                return e.ToFailedOperationResult("Unable to read inventory");
            }
        }

        public async Task<OperationResult<RealTimeInventory>> InventoryStorageFlush(string id)
        {
            await _inventoryStorage.Flush(id);
            return await ReadInventory(id);
        }

        public async Task<OperationResult<RealTimeInventory>> Reserve(string productId, int reservationQuantity)
        {
            try
            {
                if (reservationQuantity < 0)
                {
                    throw new Exception("Cannot reserve with a negative quantity of "+reservationQuantity+" for product " + productId+ GetCurrentQuantitiesReport());
                }
                var newReserved = _reservations + reservationQuantity;
                if (newReserved > _quantity - _holds) throw new Exception("What to reserve must be less than quantity - reservation for product " + productId + GetCurrentQuantitiesReport());
                var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, _quantity, newReserved, _holds));

                if (!result.IsSuccessful) throw new Exception("Could not load inventory for product " + productId + GetCurrentQuantitiesReport(), result.Errors);
                _reservations = newReserved;
                return await ReadInventory(productId);
            }
            catch (Exception e)
            {
                return e.ToFailedOperationResult();
            }
        }

        public async Task<OperationResult<RealTimeInventory>> UpdateQuantity(string productId, int quantity)
        {
            try
            {
                if (quantity < 0)
                {
                    throw new Exception("Cannot update with a negative quantity of " + quantity + " for product " + productId + GetCurrentQuantitiesReport());
                }
                var newQuantity = _quantity + quantity;

                var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, _reservations, _holds));

                if (!result.IsSuccessful) throw result.Errors.Flatten();
                _quantity = newQuantity;
                return await ReadInventory(productId);
            }
            catch (Exception e)
            {
                return e.ToFailedOperationResult();
            }
        }

        public async Task<OperationResult<RealTimeInventory>> PlaceHold(string productId, int toHold)
        {
            try
            {
                if (toHold < 0)
                {
                    throw new Exception("Cannot place a hold with a negative quantity of " + toHold + " for product " + productId + GetCurrentQuantitiesReport());
                }
                var newHolds = _holds + toHold;
                if (newHolds + _reservations > _quantity)
                {
                    throw new Exception("With the supplied hold " + toHold + ",the new hold is larger than available unreserved items for product " + productId + GetCurrentQuantitiesReport());
                }
                var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, _quantity, _reservations, newHolds));

                if (!result.IsSuccessful) throw result.Errors.Flatten();
                _holds = newHolds;
                var inventory= await ReadInventory(productId);
                return inventory;
            }
            catch (Exception e)
            {
                return e.ToFailedOperationResult();
            }
        }

        public async Task<OperationResult<RealTimeInventory>> Purchase(string productId, int quantity)
        {
            try
            {
                if (quantity < 0)
                {
                    throw new Exception("Cannot purchase with a negative quantity of " + quantity + " for product " + productId + GetCurrentQuantitiesReport());
                }
                var newQuantity = _quantity - quantity;
                var newReserved = Math.Max(0, _reservations - quantity);

                if (newQuantity - _holds < 0) throw new Exception("provided " + quantity + ", available holds must be less than or equal to quantity for product " + productId + GetCurrentQuantitiesReport());
                var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, newReserved, _holds));

                if (!result.IsSuccessful) throw result.Errors.Flatten();
                _quantity = newQuantity;
                _reservations = newReserved;
                return await ReadInventory(productId);
            }
            catch (Exception e)
            {
                return e.ToFailedOperationResult();
            }
        }

        public async Task<OperationResult<RealTimeInventory>> PurchaseFromHolds(string productId, int quantity)
        {
            try
            {
                if (quantity < 0)
                {
                    throw new Exception("Cannot purchase from holds with a negative quantity of " + quantity + " for product " + productId + GetCurrentQuantitiesReport());
                }
                var newQuantity = _quantity - quantity;
                var newHolds = _holds - quantity;

                if (newQuantity < 0 || newHolds < 0) throw new Exception("Purchase from negative hold or a negative purchase is not allowed  for product " + productId + GetCurrentQuantitiesReport());
                var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, _reservations, newHolds)).ConfigureAwait(false);

                if (!result.IsSuccessful) throw result.Errors.Flatten();
                _quantity = newQuantity;
                _holds = newHolds;
                return await ReadInventory(productId);
            }
            catch (Exception e)
            {
                return e.ToFailedOperationResult();
            }
        }
    }
}