namespace InventoryService.Messages.Response
{
    public class PurchaseFromHoldsCompletedMessage
    {
        public PurchaseFromHoldsCompletedMessage(string productId, int quantity, int reservations, int holds, bool successful)
        {
            ProductId = productId;
            Quantity = quantity;
            Successful = successful;
            Reservations = reservations;
            Holds = holds;
        }

        public InventoryOperationErrorMessage ErrorMessage { set; get; }

        public PurchaseFromHoldsCompletedMessage(InventoryOperationErrorMessage message)
        {
            ErrorMessage = message;
        }

        public string ProductId { get; private set; }
        public int Quantity { get; private set; }
        public int Reservations { get; private set; }
        public int Holds { get; private set; }
        public bool Successful { get; private set; }
    }
}