using InventoryService.Messages.Models;
using System;

namespace InventoryService.Messages.Response
{
    public class InventoryOperationErrorMessage : IInventoryServiceCompletedMessage
    {
        public InventoryOperationErrorMessage(IRealTimeInventory realTimeInventory, Exception error)
        {
            Error = error;
            RealTimeInventory = realTimeInventory;
            Successful = false;
        }

        public IRealTimeInventory RealTimeInventory { get; }
        public bool Successful { get; set; }
        public Exception Error { get; private set; }
    }
}