namespace InventoryService.Messages.Request
{
    public class GetInventoryMessage
    {
        public string ProductId { get; private set; }
        public bool GetNonStaleResult { get; private set; }
        public GetInventoryMessage(string productId, bool getNonStaleResult)
        {
            ProductId = productId;
            GetNonStaleResult = getNonStaleResult;
        }
    }
}