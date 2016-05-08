using System;

namespace InventoryService
{
	public class ReservedMessage
	{
		public string ProductId { get; private set; }
		public int ReservationQuantity { get; private set; }
		public bool Successful { get; private set; }

		public ReservedMessage (string productId, int reservationQuantity, bool successful)
		{
			ProductId = productId;
			ReservationQuantity = reservationQuantity;
			Successful = successful;
		}
	}
}

