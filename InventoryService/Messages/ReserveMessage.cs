using System;

namespace InventoryService
{
	public class ReserveMessage
	{
		public string ProductId { get; private set; }
		public int Quantity { get; private set; }

		public ReserveMessage (string productId, int quantity)
		{
			ProductId = productId;
			Quantity = quantity;
		}
	}
}

