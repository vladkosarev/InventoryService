namespace InventoryService.Messages.Request
{
    public class ReserveMessage: IRequestMessage
    {
        public ReserveMessage(string productId, int reservationQuantity)
        {
            ProductId = productId;
            Update = reservationQuantity;
        }

        public string ProductId { get; private set; }
        public int Update { get; private set; }
    }
}