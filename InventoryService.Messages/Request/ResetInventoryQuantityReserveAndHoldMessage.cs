namespace InventoryService.Messages.Request
{
    public class ResetInventoryQuantityReserveAndHoldMessage : IRequestMessage
    {
        public ResetInventoryQuantityReserveAndHoldMessage(string productId, int update, int reservations, int holds)
        {
            ProductId = productId;
            Update = update;
            Reservations = reservations;
            Holds = holds;
        }

        public string ProductId { get; }
        public int Update { get; }
        public int Reservations { get; }
        public int Holds { get; }
    }
}