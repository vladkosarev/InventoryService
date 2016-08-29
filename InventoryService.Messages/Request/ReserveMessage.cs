namespace InventoryService.Messages.Request
{
    public class ReserveMessage
    {
        public ReserveMessage(string productId, int reservationQuantity)
        {
            ProductId = productId;
            ReservationQuantity = reservationQuantity;
        }

        public string ProductId { get; private set; }
        public int ReservationQuantity { get; private set; }
    }
}