namespace InventoryService.Messages.Request
{
    public class GetInventoryMessage: IRequestMessage
    {
        public string ProductId { get; private set; }
        public int Update { get; }
        public bool GetNonStaleResult { get; private set; }

        public GetInventoryMessage(string productId, bool getNonStaleResult)
        {
            ProductId = productId;
            GetNonStaleResult = getNonStaleResult;
            Update = 0;
        }
    }
}