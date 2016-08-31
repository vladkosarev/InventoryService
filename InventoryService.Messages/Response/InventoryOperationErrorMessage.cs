using System;
using System.Collections.Generic;

namespace InventoryService.Messages.Response
{
    public class InventoryOperationErrorMessage:ICompletedMessage
    {
        public InventoryOperationErrorMessage(string productId = null, List<Exception> errors = null)
        {
            ProductId = productId;
            Errors = errors ?? new List<Exception>();
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
        public List<Exception> Errors { get; private set; }
    }
}