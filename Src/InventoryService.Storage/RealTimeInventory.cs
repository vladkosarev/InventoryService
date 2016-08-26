namespace InventoryService.Storage
{
    public class RealTimeInventory
    {
        public RealTimeInventory(string productId, int quantity, int reserved, int holds)
        {
            ProductId = productId;
            Quantity = quantity;
            Reservations = reserved;
            Holds = holds;
        }

        public int Quantity { private set; get; }
        public int Reservations { private set; get; }
        public int Holds { private set; get; }
        public string ProductId { private set; get; }

    }
}