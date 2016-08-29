namespace InventoryService.Messages.Response
{
    public class PlaceHoldCompletedMessage
    {
        public PlaceHoldCompletedMessage(string productId, int quantity, int reservations, int holds, bool successful)
        {
            ProductId = productId;
            Holds = holds;
            Successful = successful;
            Quantity = quantity;
            Reserved = reservations;
        }

        public InventoryOperationErrorMessage ErrorMessage { set; get; }

        public PlaceHoldCompletedMessage(InventoryOperationErrorMessage message)
        {
            ErrorMessage = message;
        }

        public int Quantity { get; private set; }
        public int Reserved { get; private set; }
        public int Holds { get; private set; }
        public string ProductId { get; private set; }
        public bool Successful { get; private set; }
    }
}