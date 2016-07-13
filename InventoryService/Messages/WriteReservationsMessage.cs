namespace InventoryService.Messages
{
	public class WriteReservationsMessage
	{
		public WriteReservationsMessage(string productId, int reservationQuantity)
		{
			ProductId = productId;
			ReservationQuantity = reservationQuantity;
		}

		public string ProductId { get; private set; }
		public int ReservationQuantity { get; private set; }
	}
}

