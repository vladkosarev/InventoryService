namespace InventoryService.Messages.Response
{
    public class PurchaseCompletedMessage
    {
        public PurchaseCompletedMessage(string productId, int quantity, int reservations, int holds, bool successful)
        {
            ProductId = productId;
            Quantity = quantity;
            Successful = successful;
            Reserved = reservations;
            Holds = holds;
        }

        public InventoryOperationErrorMessage ErrorMessage { set; get; }

        public PurchaseCompletedMessage(InventoryOperationErrorMessage message)
        {
            ErrorMessage = message;
        }

        public string ProductId { get; private set; }
        public int Quantity { get; private set; }
        public int Reserved { get; private set; }
        public int Holds { get; private set; }
        public bool Successful { get; set; }
    }
}