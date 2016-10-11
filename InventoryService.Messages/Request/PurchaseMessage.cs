namespace InventoryService.Messages.Request
{
    public class PurchaseMessage : IRequestMessage
    {
        public PurchaseMessage(string productId, int quantity)
        {
            ProductId = productId;
            Update = quantity;
        }

        public string ProductId { get; private set; }
        public int Update { get; private set; }
        public object Sender { get; set; }
    }
}