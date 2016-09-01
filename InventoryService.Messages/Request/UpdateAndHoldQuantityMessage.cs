namespace InventoryService.Messages.Request
{
    public class UpdateAndHoldQuantityMessage: IRequestMessage
    {
        public UpdateAndHoldQuantityMessage(string productId, int quantity)
        {
            ProductId = productId;
            Update = quantity;
        }

        public string ProductId { get; private set; }
        public int Update { get; private set; }
    }
}