namespace InventoryService.Messages.Response
{
    public class ReserveCompletedMessage
    {
        public ReserveCompletedMessage(string productId, int reservationQuantity, int quantity, int holds, bool successful)
        {
            ProductId = productId;
            ReservationQuantity = reservationQuantity;
            Successful = successful;
            Quantity = quantity;
            Holds = holds;
        }

        public InventoryOperationErrorMessage ErrorMessage { set; get; }

        public ReserveCompletedMessage(InventoryOperationErrorMessage message)
        {
            ErrorMessage = message;
        }

        public string ProductId { get; private set; }
        public int Quantity { get; private set; }
        public int ReservationQuantity { get; private set; }
        public int Holds { get; private set; }
        public bool Successful { get; private set; }
    }
}