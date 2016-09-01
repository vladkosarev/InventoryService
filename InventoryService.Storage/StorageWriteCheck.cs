namespace InventoryService.Storage
{
    //public class StorageWriteCheck
    //{
    //    public static void Execute(IRealTimeInventory inventoryObject)
    //    {
    //        if (inventoryObject == null) throw new ArgumentNullException(nameof(inventoryObject));
    //        if (string.IsNullOrEmpty(inventoryObject.ProductId))
    //        {
    //            throw new ArgumentNullException(nameof(inventoryObject.ProductId));
    //        }
    //        if (inventoryObject.Quantity < inventoryObject.Holds + inventoryObject.Reserved)
    //        {
    //            throw
    //              new Exception(
    //                  "inventory data does not satisfy the condition  Quantity - Holds - Reservations < 0 for product id : " +
    //                  inventoryObject.ProductId);
    //        }
    //    }
    //}
}