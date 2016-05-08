using System;

namespace InventoryService
{
	public class ReserveMessage
	{
		public string ProductId { get; private set; }
		public int ReservationQuantity { get; private set; }

		public ReserveMessage (string productId, int reservationQuantity)
		{
			ProductId = productId;
			ReservationQuantity = reservationQuantity;
		}
	}
}

