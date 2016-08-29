namespace InventoryService.Messages.Request
{
    public class UpdateQuantityMessage
    {
        public UpdateQuantityMessage(string productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }

        public string ProductId { get; private set; }
        public int Quantity { get; private set; }
    }
}