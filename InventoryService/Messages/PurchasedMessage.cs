using System;

namespace InventoryService
{
	public class PurchasedMessage
	{
		public string ProductId { get; private set; }
		public int Quantity { get; private set; }
		public bool Successful { get; private set; }

		public PurchasedMessage (string productId, int quantity, bool successful)
		{
			ProductId = productId;
			Quantity = quantity;
			Successful = successful;
		}
	}
}


