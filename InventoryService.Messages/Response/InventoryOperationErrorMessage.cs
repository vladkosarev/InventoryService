using System;

namespace InventoryService.Messages.Response
{
    public class InventoryOperationErrorMessage : IInventoryServiceCompletedMessage
    {
        public InventoryOperationErrorMessage(int quantity, int reserved, int holds, string productId = null, AggregateException error = null)
        {
            Quantity = quantity;
            Reserved = reserved;
            Holds = holds;
            ProductId = productId;
            Error = error ?? new AggregateException();
           
            Successful = false;
        }

        public string ProductId { get; private set; }
        public int Quantity { get; }
        public int Reserved { get; }
        public int Holds { get; }
        public bool Successful { get; set; }
        public AggregateException Error { get; private set; }
    }
}