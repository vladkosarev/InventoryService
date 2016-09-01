using System;
using InventoryService.Messages.Models;

namespace InventoryService.Messages.Response
{
    public class InventoryOperationErrorMessage : IInventoryServiceCompletedMessage
    {
        public InventoryOperationErrorMessage(IRealTimeInventory realTimeInventory, AggregateException error )
        {
           
         
            Error = error ?? new AggregateException();
            RealTimeInventory = realTimeInventory;
            Successful = false;
        }

      
        public IRealTimeInventory RealTimeInventory { get; }
        public bool Successful { get; set; }
        public AggregateException Error { get; private set; }
    }
}