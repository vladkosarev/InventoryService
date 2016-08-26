namespace InventoryService.Messages
{
	public class WriteInventoryMessage
	{
		public WriteInventoryMessage(string productId, int quantity, int reservationQuantity)
		{
			ProductId = productId;
			Quantity = quantity;
			ReservationQuantity = reservationQuantity;
		}

		public string ProductId { get; private set; }
		public int Quantity { get; private set; }
		public int ReservationQuantity { get; private set; }
	}
}

