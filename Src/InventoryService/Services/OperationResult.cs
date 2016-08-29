using System;

namespace InventoryService.Services
{
    public class OperationResult<T>
    {
        public bool IsSuccessful { set; get; }
        public T Result { set; get; }
        public Exception Exception { get; set; }
    }
}