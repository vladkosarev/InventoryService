﻿namespace InventoryService.Messages
{
    public class RetrieveInventoryCompletedMessage
    {
        public string ProductId { get; private set; }
        public int Quantity { get; private set; }
        public int Reservations { get; private set; }

        public RetrieveInventoryCompletedMessage(string productId, int quantity, int reservations)
        {
            ProductId = productId;
            Quantity = quantity;
            Reservations = reservations;
        }
    }
}