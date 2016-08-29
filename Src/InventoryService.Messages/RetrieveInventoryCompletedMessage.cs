namespace InventoryService.Messages
{
    public class RetrieveInventoryCompletedMessage
    {
        public string ProductId { get; private set; }
        public int Quantity { get; private set; }
        public int Reservations { get; private set; }
        public int Holds { get; private set; }

        public RetrieveInventoryCompletedMessage(string productId, int quantity, int reservations, int holds)
        {
            ProductId = productId;
            Quantity = quantity;
            Reservations = reservations;
            Holds = holds;
        }
    }
}