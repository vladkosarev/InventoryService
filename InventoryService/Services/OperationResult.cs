using System;

namespace InventoryService.Services
{
    public class OperationResult<T>
    {
        public bool IsSuccessful { set; get; }
        public T Data { set; get; }
        public Exception Exception { get; set; }
    }
}