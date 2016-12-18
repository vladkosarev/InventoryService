using System;

namespace InventoryService.Messages.Models
{
    public interface IRealTimeInventory
    {
        int Reserved { get; }
        int Quantity { get; }
        int Holds { get; }
        string ProductId { get; }
        Guid? ETag { get; }
    }
}