namespace InventoryService.Messages
{
    public class PlaceHoldCompletedMessage
    {
        public PlaceHoldCompletedMessage(string productId, int holds, bool successful)
        {
            ProductId = productId;
            Holds = holds;
            Successful = successful;
        }

        public string ProductId { get; private set; }
        public int Holds { get; private set; }
        public bool Successful { get; private set; }
    }
}