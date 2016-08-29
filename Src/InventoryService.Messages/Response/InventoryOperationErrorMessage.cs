using System;
using System.Collections.Generic;

namespace InventoryService.Messages.Response
{
    public class InventoryOperationErrorMessage
    {
        public InventoryOperationErrorMessage(string productId = null, List<Exception> errors = null)
        {
            ProductId = productId;
            Errors = Errors ?? new List<Exception>();
        }

        public string ProductId { get; private set; }
        public List<Exception> Errors { get; private set; }
        public bool IsSuccessful => Errors != null && Errors.Count > 0;
    }
}