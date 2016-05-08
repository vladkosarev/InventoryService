using System;

namespace InventoryService
{
	public class PurchaseMessage
	{
		public string ProductId { get; private set; }
		public int Quantity { get; private set; }

		public PurchaseMessage (string productId, int quantity)
		{
			ProductId = productId;
			Quantity = quantity;
		}
	}
}


