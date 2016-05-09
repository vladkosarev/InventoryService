namespace InventoryService.Messages
{
    public class LoadedInventoryMessage
    {
        public LoadedInventoryMessage(int quantity, int reservedQuantity)
        {
            Quantity = quantity;
            ReservedQuantity = reservedQuantity;
        }

        public int Quantity { get; private set; }
        public int ReservedQuantity { get; private set; }
    }
}