namespace InventoryService.Messages
{
    public class GetInventoryMessage
    {
        public string ProductId { get; private set; }

        public GetInventoryMessage(string productId)
        {
            ProductId = productId;
        }
    }
}