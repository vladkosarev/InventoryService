using System;
using System.Collections.Generic;

namespace InventoryService.Messages.Response
{
    public class InventoryOperationErrorMessage
    {
        public InventoryOperationErrorMessage(string productId = null, List<Exception> errors = null)
        {
            ProductId = productId;
            Errors = errors ?? new List<Exception>();
        }

        public string ProductId { get; private set; }
        public List<Exception> Errors { get; private set; }
    }
}