using System;
using System.Collections.Generic;

namespace InventoryService.Messages.Response
{
    public class InventoryOperationErrorMessage:IInventoryServiceCompletedMessage
    {
        public InventoryOperationErrorMessage(string productId = null, AggregateException error = null)
        {
            ProductId = productId;
            Error = error ?? new AggregateException();
            Quantity = 0;
            Reserved = 0;
            Holds = 0;
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