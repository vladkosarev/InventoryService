namespace InventoryService.Messages.Request
{
    public interface IRequestMessage
    {
        string ProductId { get;  }
        int Update { get; }
    }
}