namespace InventoryService.Messages
{
    public class PlaceHoldMessage
    {
        public PlaceHoldMessage(string productId, int hold)
        {
            ProductId = productId;
            Holds = hold;
        }

        public string ProductId { get; private set; }
        public int Holds { get; private set; }
    }
}