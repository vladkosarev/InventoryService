namespace InventoryService.Messages.Request
{
    public class PlaceHoldMessage: IRequestMessage
    {
        public PlaceHoldMessage(string productId, int hold)
        {
            ProductId = productId;
            Update = hold;
        }

        public string ProductId { get; private set; }
        public int Update { get; private set; }
    }
}