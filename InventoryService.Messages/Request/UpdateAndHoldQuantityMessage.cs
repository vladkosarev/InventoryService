namespace InventoryService.Messages.Request
{
    public class UpdateAndHoldQuantityMessage
    {
        public UpdateAndHoldQuantityMessage(string productId, int quantity)
        {
            ProductId = productId;
            Quantity = quantity;
        }

        public string ProductId { get; private set; }
        public int Quantity { get; private set; }
    }
}