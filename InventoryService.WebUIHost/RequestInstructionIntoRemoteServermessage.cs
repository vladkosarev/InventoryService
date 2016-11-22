using InventoryService.Messages;

namespace InventoryService.WebUIHost
{
    public class RequestInstructionIntoRemoteServermessage
    {
        public RequestInstructionIntoRemoteServermessage(IRequestMessage message, int numberOfTimes)
        {
            Message = message;
            NumberOfTimes = numberOfTimes;
        }

        public RequestInstructionIntoRemoteServermessage(string operationName, string productId, int quantity, int numberOfTimes)
        {
            OperationName = operationName;
            ProductId = productId;
            Quantity = quantity;
            NumberOfTimes = numberOfTimes;
        }

        public IRequestMessage Message {  get; }

        public string OperationName { get; }
        public string ProductId { get; }
        public int Quantity { get; }

        public int NumberOfTimes { get; }
    }
}