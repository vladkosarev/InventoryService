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
            ETag = Guid.NewGuid();
        }

        public int Quantity { get; }
        public int Reserved { get; }
        public int Holds { get; }
        public string ProductId { get; }
        public Guid ETag { get; }
    }

    //public class RealTimeInventory : IRealTimeInventory
    //{
    //    public RealTimeInventory(string productId, int quantity, int reserved, int holds, string metaData = null)
    //    {
    //        ProductId = productId;
    //        Quantity = quantity;
    //        Reserved = reserved;
    //        Holds = holds;
    //        CreatedAt = DateTime.UtcNow;
    //        MetaData = metaData;
    //    }

    //    public int Quantity { get; }
    //    public int Reserved { get; }
    //    public int Holds { get; }
    //    public string ProductId { get; }
    //    public string MetaData { get; }
    //    public DateTime CreatedAt { get; }
    //}
}