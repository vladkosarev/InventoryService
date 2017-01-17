using InventoryService.Messages;

namespace InventoryService.WebUIHost
{
    public class RequestInstructionIntoRemoteServermessage
    {
        public RequestInstructionIntoRemoteServermessage(IRequestMessage message, int retryCount)
        {
            Message = message;
            RetryCount = retryCount;
        }

        public RequestInstructionIntoRemoteServermessage(string operationName, string productId, int quantity, int retryCount)
        {
            OperationName = operationName;
            ProductId = productId;
            Quantity = quantity;
            RetryCount = retryCount;
        }

        public IRequestMessage Message { get; }

        public string OperationName { get; }
        public string ProductId { get; }
        public int Quantity { get; }

        public int RetryCount { get; }
    }
}