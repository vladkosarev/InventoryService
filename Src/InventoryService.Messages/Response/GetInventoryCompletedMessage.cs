namespace InventoryService.Messages.Response
{
    public class GetInventoryCompletedMessage
    {
        public string ProductId { get; private set; }
        public int Quantity { get; private set; }
        public int Reservations { get; private set; }
        public int Holds { get; private set; }
        public InventoryOperationErrorMessage ErrorMessage { set; get; }

        public GetInventoryCompletedMessage(InventoryOperationErrorMessage message)
        {
            ErrorMessage = message;
        }

        public GetInventoryCompletedMessage(string productId, int quantity, int reservations, int holds)
        {
            ProductId = productId;
            Quantity = quantity;
            Reservations = reservations;
            Holds = holds;
        }
    }
}