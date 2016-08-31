﻿namespace InventoryService.Messages.Models
{
    public interface IRealTimeInventory
    {
        int Reserved { get; }
        int Quantity { get; }
        int Holds { get; }
        string ProductId { get; }
    }

    public class RealTimeInventory : IRealTimeInventory
    {
        public RealTimeInventory(string productId, int quantity, int reserved, int holds)
        {
            ProductId = productId;
            Quantity = quantity;
            Reserved = reserved;
            Holds = holds;
        }

        public int Quantity { get; }
        public int Reserved { private set; get; }
        public int Holds { get; }
        public string ProductId { get; }
    }
}