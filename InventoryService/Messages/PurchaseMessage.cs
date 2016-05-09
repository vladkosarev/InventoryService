namespace InventoryService.Messages
{
    public class PurchaseMessage
    {
        public PurchaseMessage(string productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }

        public string ProductId { get; private set; }
        public int Quantity { get; private set; }
    }
}