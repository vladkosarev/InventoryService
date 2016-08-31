namespace InventoryService.Messages.Response
{
    public interface IInventoryServiceCompletedMessage
    {
        string ProductId { get; }
        int Quantity { get; }
        int Reserved { get; }
        int Holds { get; }
        bool Successful { get; }
    }
}