using System;

namespace InventoryService.Messages.Models
{
    public class RealTimeInventory : IRealTimeInventory
    {
        public RealTimeInventory(string productId, int quantity, int reserved, int holds)
        {
            ProductId = productId;
            Quantity = quantity;
            Reserved = reserved;
            Holds = holds;
            ETag = this.GenerateNextGuid();
            UpdatedOn = DateTime.UtcNow;
        }

        public int Quantity { get; }
        public int Reserved { get; }
        public int Holds { get; }
        public string ProductId { get; }
        public Guid? ETag { get; }
        public DateTime UpdatedOn { get; }
    }
}