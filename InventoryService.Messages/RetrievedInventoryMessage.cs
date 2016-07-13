namespace InventoryService.Messages
{
    public class RetrievedInventoryMessage
    {
        public string ProductId { get; private set; }
        public int Quantity { get; private set; }
        public int Reservations { get; private set; }

        public RetrievedInventoryMessage(string productId, int quantity, int reservations)
        {
            ProductId = productId;
            Quantity = quantity;
            Reservations = reservations;
        }
    }
}