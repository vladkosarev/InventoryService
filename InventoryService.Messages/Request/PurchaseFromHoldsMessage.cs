namespace InventoryService.Messages.Request
{
    public class PurchaseFromHoldsMessage: IRequestMessage
    {
        public PurchaseFromHoldsMessage(string productId, int quantity)
        {
            ProductId = productId;
            Update = quantity;
        }

        public string ProductId { get; private set; }
        public int Update { get; private set; }
    }
}