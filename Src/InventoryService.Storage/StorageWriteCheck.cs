using System;
using System.Threading.Tasks;

namespace InventoryService.Storage
{
    public class StorageWriteCheck
    {
        public static async void Execute(RealTimeInventory inventoryObject)
        {
            if (inventoryObject == null) throw new ArgumentNullException(nameof(inventoryObject));
            if ( string.IsNullOrEmpty(inventoryObject.ProductId))
            {
                throw new ArgumentNullException(nameof(inventoryObject.ProductId));
            }
            if (inventoryObject.Quantity - inventoryObject.Holds - inventoryObject.Reservations < 0)
            {
                await Task.FromResult(new StorageOperationResult()
                {
                    IsSuccessful = false,
                    Errors = new AggregateException("inventory data does not satisfy the condition  Quantity - Holds - Reservations < 0 for product id : " + inventoryObject.ProductId)
                });
            }
        }
    }
}