namespace InventoryService.Messages.Response
{
    public class ReserveCompletedMessage : IInventoryServiceCompletedMessage
    {
        public ReserveCompletedMessage(string productId, int quantity, int reservationQuantity, int holds, bool successful)
        {
            ProductId = productId;
            Reserved = reservationQuantity;
            Successful = successful;
            Quantity = quantity;
            Holds = holds;
            Successful = true;
        }
        

        public string ProductId { get; private set; }
        public int Quantity { get; private set; }
        public int Reserved { get; private set; }
        public int Holds { get; private set; }
        public bool Successful { get;   }
    }
}