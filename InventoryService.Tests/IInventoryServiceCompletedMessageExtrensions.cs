using InventoryService.Messages.Response;

namespace InventoryService.Tests
{
    public static class IInventoryServiceCompletedMessageExtrensions
    {
        public static  Inventory ToInventory(this IInventoryServiceCompletedMessage inventorymessage)
        {
            return  new Inventory(inventorymessage.ProductId, inventorymessage.Quantity,inventorymessage.Reserved,inventorymessage.Holds);
        }
    }
}