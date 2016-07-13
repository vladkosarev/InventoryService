namespace InventoryService.Messages
{
    public class FlushStreamMessage
    {
        public string ProductId { get; private set; }
        public FlushStreamMessage(string productId)
        {
            ProductId = productId;
        }
    }
}
