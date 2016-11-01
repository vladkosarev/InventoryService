using InventoryService.Messages;
using InventoryService.Messages.Models;
using InventoryService.Storage;
using System;
using System.Threading.Tasks;

namespace InventoryService
{
    //todo return the full error message
    public static class ProductInventoryOperations
    {
        public static RealTimeInventory InitializeFromStorage(this RealTimeInventory realTimeInventory, IInventoryStorage inventoryStorage, string id)
        {
            var initTask = inventoryStorage.ReadInventoryAsync(id);
            Task.WaitAll(initTask);
            var inventory = initTask.Result.Result;

            if (!initTask.Result.IsSuccessful)
            {
                throw initTask.Result.Errors.Flatten();
            }
            return new RealTimeInventory(id, inventory.Quantity, inventory.Reserved, inventory.Holds);
        }

        private static OperationResult<IRealTimeInventory> ToOperationResult(this IRealTimeInventory realTimeInventory, bool isSuccessful)
        {
            return new OperationResult<IRealTimeInventory>()
            {
                Data = realTimeInventory,
                IsSuccessful = isSuccessful
            };
        }

        public static async Task<OperationResult<IRealTimeInventory>> ReadInventoryFromStorageAsync(this IRealTimeInventory realTimeInventory, IInventoryStorage inventoryStorage, string productId)
        {
            try
            {
                if (string.IsNullOrEmpty(productId)) return InventoryServiceErrorMessageGenerator.Generate(ErrorType.NO_PRODUCT_ID_SPECIFIED, realTimeInventory, 0).ToFailedOperationResult(realTimeInventory);
                var result = await inventoryStorage.ReadInventoryAsync(productId);
                return result.Result.ToSuccessOperationResult();
            }
            catch (Exception e)
            {
                return InventoryServiceErrorMessageGenerator.Generate(ErrorType.UNABLE_TO_READ_INV, realTimeInventory, 0, e).ToFailedOperationResult();
            }
        }

        public static async Task<OperationResult<IRealTimeInventory>> InventoryStorageFlushAsync(this IRealTimeInventory realTimeInventory, IInventoryStorage inventoryStorage, string id)
        {
            await inventoryStorage.FlushAsync();
            return realTimeInventory.ToOperationResult(isSuccessful: true);
        }

        public static async Task<OperationResult<IRealTimeInventory>> ReserveAsync(this IRealTimeInventory realTimeInventory, IInventoryStorage inventoryStorage, string productId, int reservationQuantity)
        {
            var newReserved = Math.Max(0, realTimeInventory.Reserved + reservationQuantity);

            if ((reservationQuantity > 0) && (newReserved > realTimeInventory.Quantity - realTimeInventory.Holds))
                return InventoryServiceErrorMessageGenerator.Generate(ErrorType.RESERVATION_EXCEED_QUANTITY, realTimeInventory, reservationQuantity).ToFailedOperationResult(realTimeInventory, productId);

            var newRealTimeInventory = new RealTimeInventory(productId, realTimeInventory.Quantity, newReserved,
                realTimeInventory.Holds);
            var result =
                await
                    inventoryStorage.WriteInventoryAsync(newRealTimeInventory);

            if (!result.IsSuccessful)
                return InventoryServiceErrorMessageGenerator.Generate(ErrorType.UNABLE_TO_UPDATE_INVENTORY_STORAGE, realTimeInventory, reservationQuantity, result.Errors).ToFailedOperationResult(realTimeInventory, productId);

            return newRealTimeInventory.ToOperationResult(isSuccessful: true);
        }

        public static async Task<OperationResult<IRealTimeInventory>> UpdateQuantityAsync(this IRealTimeInventory realTimeInventory, IInventoryStorage inventoryStorage, string productId, int quantity)
        {
            var newQuantity = realTimeInventory.Quantity + quantity;

            var newRealTimeInventory = new RealTimeInventory(productId, newQuantity, realTimeInventory.Reserved, realTimeInventory.Holds);
            var result = await inventoryStorage.WriteInventoryAsync(newRealTimeInventory);

            if (!result.IsSuccessful)
            {
                return InventoryServiceErrorMessageGenerator.Generate(ErrorType.UNABLE_TO_UPDATE_INVENTORY_STORAGE, realTimeInventory, quantity, result.Errors).ToFailedOperationResult(realTimeInventory, productId);
            }

            return newRealTimeInventory.ToOperationResult(isSuccessful: true);
        }



        public static async Task<OperationResult<IRealTimeInventory>> ResetInventoryQuantityReserveAndHoldAsync(this IRealTimeInventory currentInventory, IInventoryStorage inventoryStorage, string productId, int quantity,int reserve, int hold)
        {
            IRealTimeInventory realTimeInventory = new RealTimeInventory(currentInventory.ProductId, 0, 0, 0);

            var updateQuantityResult=await realTimeInventory.UpdateQuantityAsync(inventoryStorage, productId, quantity);
            if (!updateQuantityResult.IsSuccessful)
            {
                updateQuantityResult.Data = currentInventory;
                return updateQuantityResult;
            }

            var reserveResult=   await updateQuantityResult.Data.ReserveAsync(inventoryStorage, productId, reserve);
            if (!reserveResult.IsSuccessful)
            {
                reserveResult.Data = currentInventory;
                return reserveResult;
            }

            var placeHoldResult=     await reserveResult.Data.PlaceHoldAsync(inventoryStorage, productId, hold);
            if (!placeHoldResult.IsSuccessful)
            {
                placeHoldResult.Data = currentInventory;
                return placeHoldResult;
            }

            return placeHoldResult.Data.ToOperationResult(isSuccessful: true);
        }





        public static async Task<OperationResult<IRealTimeInventory>> UpdateQuantityAndHoldAsync(this IRealTimeInventory realTimeInventory, IInventoryStorage inventoryStorage, string productId, int quantity)
        {
            var newQuantity = realTimeInventory.Quantity + quantity;
            var newHolds = realTimeInventory.Holds + quantity;
            if (newHolds > newQuantity)
            {
                return InventoryServiceErrorMessageGenerator.Generate(ErrorType.HOLD_EXCEED_QUANTITY_FOR_UPDATEQUANTITYANDHOLD, realTimeInventory, quantity).ToFailedOperationResult(realTimeInventory, productId);
            }
            var newRealTimeInventory = new RealTimeInventory(productId, newQuantity, realTimeInventory.Reserved, newHolds);
            var result = await inventoryStorage.WriteInventoryAsync(newRealTimeInventory);

            if (!result.IsSuccessful) return InventoryServiceErrorMessageGenerator.Generate(ErrorType.UNABLE_TO_UPDATE_INVENTORY_STORAGE, realTimeInventory, quantity, result.Errors).ToFailedOperationResult(realTimeInventory, productId);

            return newRealTimeInventory.ToOperationResult(isSuccessful: true);
        }

        public static async Task<OperationResult<IRealTimeInventory>> PlaceHoldAsync(this IRealTimeInventory realTimeInventory, IInventoryStorage inventoryStorage, string productId, int toHold)
        {
            var newHolds = realTimeInventory.Holds + toHold;
            if (newHolds > realTimeInventory.Quantity)
            {
                return InventoryServiceErrorMessageGenerator.Generate(ErrorType.HOLD_EXCEED_QUANTITY_FOR_HOLD, realTimeInventory, toHold).ToFailedOperationResult(realTimeInventory, productId);
            }
            var newRealTimeInventory = new RealTimeInventory(productId, realTimeInventory.Quantity, realTimeInventory.Reserved, newHolds);

            var result = await inventoryStorage.WriteInventoryAsync(newRealTimeInventory);

            if (!result.IsSuccessful) return InventoryServiceErrorMessageGenerator.Generate(ErrorType.UNABLE_TO_UPDATE_INVENTORY_STORAGE, realTimeInventory, toHold, result.Errors).ToFailedOperationResult(realTimeInventory, productId);

            return newRealTimeInventory.ToOperationResult(isSuccessful: true);
        }

        public static async Task<OperationResult<IRealTimeInventory>> PurchaseAsync(this IRealTimeInventory realTimeInventory, IInventoryStorage inventoryStorage, string productId, int quantity)
        {
            if (quantity < 0)
            {
                return InventoryServiceErrorMessageGenerator.Generate(ErrorType.NEGATIVE_PURCHASE_FOR_PURCHASEFROMRESERVATION, realTimeInventory, quantity).ToFailedOperationResult(realTimeInventory, productId);
            }
            var newQuantity = realTimeInventory.Quantity - quantity;

            var newReserved = Math.Max(0, realTimeInventory.Reserved - quantity);
            //todo we still want to sell even though there is reservation
            //var newReserved = realTimeInventory.Reserved - quantity;
            //if(newReserved<0) throw new Exception("provided " + quantity + ", available reservations must be less than or equal to quantity for product " + productId);

            if (newQuantity - realTimeInventory.Holds < 0) return InventoryServiceErrorMessageGenerator.Generate(ErrorType.PURCHASE_EXCEED_QUANTITY_FOR_PURCHASEFROMRESERVATION, realTimeInventory, quantity).ToFailedOperationResult(realTimeInventory, productId);

            var newrealTimeInventory = new RealTimeInventory(productId, newQuantity, newReserved, realTimeInventory.Holds);
            var result = await inventoryStorage.WriteInventoryAsync(newrealTimeInventory);

            if (!result.IsSuccessful) return InventoryServiceErrorMessageGenerator.Generate(ErrorType.UNABLE_TO_UPDATE_INVENTORY_STORAGE, realTimeInventory, quantity, result.Errors).ToFailedOperationResult(realTimeInventory, productId);

            return newrealTimeInventory.ToOperationResult(isSuccessful: true);
        }

        public static async Task<OperationResult<IRealTimeInventory>> PurchaseFromHoldsAsync(this IRealTimeInventory realTimeInventory, IInventoryStorage inventoryStorage, string productId, int quantity)
        {
            if (quantity < 0)
            {
                return InventoryServiceErrorMessageGenerator.Generate(ErrorType.NEGATIVE_PURCHASE_FOR_PURCHASEFROMHOLD, realTimeInventory, quantity).ToFailedOperationResult(realTimeInventory, productId);
            }
            var newQuantity = realTimeInventory.Quantity - quantity;
            var newHolds = realTimeInventory.Holds - quantity;

            if (newQuantity < 0 || newHolds < 0) return InventoryServiceErrorMessageGenerator.Generate(ErrorType.PURCHASE_EXCEED_QUANTITY_FOR_PURCHASEFROMHOLD, realTimeInventory, quantity).ToFailedOperationResult(realTimeInventory, productId);

            var newrealTimeInventory = new RealTimeInventory(productId, newQuantity, realTimeInventory.Reserved,
                newHolds);

            var result = await inventoryStorage.WriteInventoryAsync(newrealTimeInventory).ConfigureAwait(false);

            if (!result.IsSuccessful) return InventoryServiceErrorMessageGenerator.Generate(ErrorType.UNABLE_TO_UPDATE_INVENTORY_STORAGE, realTimeInventory, quantity, result.Errors).ToFailedOperationResult(realTimeInventory, productId);

            return newrealTimeInventory.ToOperationResult(isSuccessful: true);
        }
    }
}