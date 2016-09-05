namespace InventoryService.Messages
{
    public interface IRequestMessage
    {
        string ProductId { get; }
        int Update { get; }
    }
}