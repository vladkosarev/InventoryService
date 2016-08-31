namespace InventoryService.Messages.Response
{
    public class GetInventoryCompletedMessage: ICompletedMessage
    {
        public string ProductId { get; private set; }
        public int Quantity { get; private set; }
        public int Reserved { get; private set; }
        public int Holds { get; private set; }
        public bool Successful { get; set; }

        public GetInventoryCompletedMessage(string productId, int quantity, int reservations, int holds)
        {
            ProductId = productId;
            Quantity = quantity;
            Reserved = reservations;
            Holds = holds;
        }
    }
}