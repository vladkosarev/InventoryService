namespace InventoryService.Messages.Response
{
    public class PlaceHoldCompletedMessage: ICompletedMessage
    {
        public PlaceHoldCompletedMessage(string productId, int quantity, int reservations, int holds, bool successful)
        {
            ProductId = productId;
            Holds = holds;
            Successful = successful;
            Quantity = quantity;
            Reserved = reservations;
        }
        

        public int Quantity { get; private set; }
        public int Reserved { get; private set; }
        public int Holds { get; private set; }
        public string ProductId { get; private set; }
        public bool Successful { get; set; }
    }
}