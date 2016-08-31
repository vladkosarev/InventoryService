using InventoryService.Messages.Models;
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

            if (!initTask.Result.IsSuccessful)
            {
                throw initTask.Result.Errors.Flatten();
            }
            _quantity = inventory.Quantity;
            _reservations = inventory.Reserved;
            _holds = inventory.Holds;
        }

        public async Task<OperationResult<IRealTimeInventory>> ReadInventory(string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(productId)) throw new ArgumentNullException(nameof(productId));
                var result = await _inventoryStorage.ReadInventory(productId);
                return result.Result.ToSuccessOperationResult();
            }
            catch (Exception e)
            {
                return e.ToFailedOperationResult("Unable to read inventory for product " + productId);
            }
        }

        public async Task<OperationResult<IRealTimeInventory>> InventoryStorageFlush(string id)
        {
            await _inventoryStorage.Flush(id);
            return await ReadInventory(id);
        }

        public async Task<OperationResult<IRealTimeInventory>> Reserve(string productId, int reservationQuantity)
        {
            return await ExecuteAndGetInventory(productId, async () =>
            {
                if (reservationQuantity < 0)
                {
                    throw new Exception("Cannot reserve with a negative quantity of " + reservationQuantity +
                                        " for product " + productId);
                }
                var newReserved = _reservations + reservationQuantity;
                if (newReserved > _quantity - _holds)
                    throw new Exception("What to reserve must be less than quantity - reservation for product " +
                                        productId);
                var result =
                    await
                        _inventoryStorage.WriteInventory(new RealTimeInventory(productId, _quantity, newReserved, _holds));

                if (!result.IsSuccessful)
                    throw new Exception("Could not load inventory for product " + productId, result.Errors);
                _reservations = newReserved;

                return true;
            });
        }

        public async Task<OperationResult<IRealTimeInventory>> UpdateQuantity(string productId, int quantity)
        {
            return await ExecuteAndGetInventory(productId, async () =>
            {
                if (quantity < 0)
                {
                    throw new Exception("Cannot update with a negative quantity of " + quantity + " for product " + productId);
                }
                var newQuantity = _quantity + quantity;

                var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, _reservations, _holds));

                if (!result.IsSuccessful) throw result.Errors.Flatten();
                _quantity = newQuantity;

                return true;
            });
        }

        public async Task<OperationResult<IRealTimeInventory>> PlaceHold(string productId, int toHold)
        {
            return await ExecuteAndGetInventory(productId, async () =>
            {
                if (toHold < 0)
                {
                    throw new Exception("Cannot place a hold with a negative quantity of " + toHold + " for product " + productId);
                }
                var newHolds = _holds + toHold;
                if (newHolds + _reservations > _quantity)
                {
                    throw new Exception("With the supplied hold " + toHold + ",the new hold is larger than available unreserved items for product " + productId);
                }
                var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, _quantity, _reservations, newHolds));

                if (!result.IsSuccessful) throw result.Errors.Flatten();
                _holds = newHolds;
                return true;
            });
        }

        public async Task<OperationResult<IRealTimeInventory>> Purchase(string productId, int quantity)
        {
            return await ExecuteAndGetInventory(productId, async () =>
            {
                if (quantity < 0)
                {
                    throw new Exception("Cannot purchase with a negative quantity of " + quantity + " for product " + productId);
                }
                var newQuantity = _quantity - quantity;
                var newReserved = Math.Max(0, _reservations - quantity);

                if (newQuantity - _holds < 0) throw new Exception("provided " + quantity + ", available holds must be less than or equal to quantity for product " + productId);
                var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, newReserved, _holds));

                if (!result.IsSuccessful) throw result.Errors.Flatten();
                _quantity = newQuantity;
                _reservations = newReserved;

                return true;
            });
        }

        public async Task<OperationResult<IRealTimeInventory>> PurchaseFromHolds(string productId, int quantity)
        {
            return await ExecuteAndGetInventory(productId, async () =>
                 {
                     if (quantity < 0)
                     {
                         throw new Exception("Cannot purchase from holds with a negative quantity of " + quantity + " for product " + productId);
                     }
                     var newQuantity = _quantity - quantity;
                     var newHolds = _holds - quantity;

                     if (newQuantity < 0 || newHolds < 0) throw new Exception("Purchase from negative hold or a negative purchase is not allowed  for product " + productId);
                     var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, _reservations, newHolds)).ConfigureAwait(false);

                     if (!result.IsSuccessful) throw result.Errors.Flatten();
                     _quantity = newQuantity;
                     _holds = newHolds;

                     return true;
                 });
        }

        private async Task<OperationResult<IRealTimeInventory>> ExecuteAndGetInventory(string productId, Func<Task<bool>> opearation)
        {
            Exception exception = null;
            OperationResult<IRealTimeInventory> finalResult = null;
            try
            {
                await opearation();
            }
            catch (Exception e)
            {
                exception = e;
            }
            finally
            {
                var currentInventory = await ReadInventory(productId);
                finalResult = exception != null ? exception.ToFailedOperationResult(currentInventory.Data) : currentInventory;
            }
            return finalResult;
        }
    }
}