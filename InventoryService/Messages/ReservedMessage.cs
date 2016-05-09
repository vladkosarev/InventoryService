namespace InventoryService.Messages
{
    public class ReservedMessage
    {
        public ReservedMessage(string productId, int reservationQuantity, bool successful)
        {
            ProductId = productId;
            ReservationQuantity = reservationQuantity;
            Successful = successful;
        }

        public string ProductId { get; private set; }
        public int ReservationQuantity { get; private set; }
        public bool Successful { get; private set; }
    }
}