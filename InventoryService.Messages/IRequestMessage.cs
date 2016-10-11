namespace InventoryService.Messages
{
    public interface IRequestMessage
    {
        string ProductId { get; }
        int Update { get; }
        // make sender mutable so message can be redirected
        object Sender { set; get; }
    }
}