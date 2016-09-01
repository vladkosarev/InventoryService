namespace InventoryService.Messages.Request
{
    public class GetInventoryMessage : IRequestMessage
    {
        public string ProductId { get; private set; }
        public int Update { get; }

        public GetInventoryMessage(string productId)
        {
            ProductId = productId;
            Update = 0;
        }
    }
}