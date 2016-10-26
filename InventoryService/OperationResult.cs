using InventoryService.Messages;

namespace InventoryService
{
    public class OperationResult<T>
    {
        public bool IsSuccessful { set; get; }
        public T Data { set; get; }
        public RealTimeInventoryException Exception { get; set; }
    }
}