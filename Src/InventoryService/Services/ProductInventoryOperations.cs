using InventoryService.Messages.Response;
using InventoryService.Storage;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InventoryService.Services
{
    public static class RealTimeInventoryOPerationalResultExtensions
    {
        public static void ThrowIfFailedOperationResult(
            this OperationResult<RealTimeInventory> realTimeInventoryOperationResult)
        {
            if (realTimeInventoryOperationResult.IsSuccessful) return;
            if (realTimeInventoryOperationResult.Exception != null)
            {
                throw realTimeInventoryOperationResult.Exception;
            }
            var inventoryProductId = realTimeInventoryOperationResult?.Result?.ProductId;
            throw new Exception("Inventory operation failed" + (string.IsNullOrEmpty(inventoryProductId) ? "" : inventoryProductId));
        }

        public static OperationResult<RealTimeInventory> ToSuccessOperationResult(
            this RealTimeInventory realTimeInventory)
        {
            return new OperationResult<RealTimeInventory>()
            {
                Result = realTimeInventory,
                IsSuccessful = true
            };
        }

        public static OperationResult<RealTimeInventory> ToFailedOperationResult(
            this Exception exception, string message = "Inventory operation failed")
        {
            return new OperationResult<RealTimeInventory>()
            {
                Result = null,
                IsSuccessful = true,
                Exception = new Exception(message, exception)
            };
        }

        public static InventoryOperationErrorMessage ToInventoryOperationErrorMessage(
           this Exception exception, string productId, string message = "Inventory operation failed")
        {
            return new InventoryOperationErrorMessage(productId, new List<Exception>()
            {
                new Exception(message, exception)
            });
        }
    }

    public class OperationResult<T>
    {
        public bool IsSuccessful { set; get; }
        public T Result { set; get; }
        public Exception Exception { get; set; }
    }

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

        public async Task<OperationResult<RealTimeInventory>> ReadInventory(string productId)
        {
            try
            {
                var result = await _inventoryStorage.ReadInventory(productId);
                return result.ToSuccessOperationResult();
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
                var newReserved = _reservations + reservationQuantity;
                if (newReserved > _quantity - _holds) return null;
                var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, _quantity, newReserved, _holds));

                if (!result) return null;
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
                var newQuantity = _quantity + quantity;

                var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, _reservations, _holds));

                if (!result) return null;
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
                var newHolds = _holds + toHold;
                if (newHolds > _quantity) return null;
                var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, _quantity, _reservations, newHolds));

                if (!result) return null;
                _holds = newHolds;
                return await ReadInventory(productId);
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
                var newQuantity = _quantity - quantity;
                var newReserved = Math.Max(0, _reservations - quantity);

                if (newQuantity - _holds < 0) return null;
                var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, newReserved, _holds));

                if (!result) return null;
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
                var newQuantity = _quantity - quantity;
                var newHolds = _holds - quantity;

                if (newQuantity < 0 || newHolds < 0) return null;
                var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, _reservations, newHolds));

                if (!result) return null;
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