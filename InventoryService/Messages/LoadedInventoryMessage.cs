using System;

namespace InventoryService
{
	public class LoadedInventoryMessage {
		public int Quantity { get; private set; }
		public int ReservedQuantity { get; private set; }

		public LoadedInventoryMessage (int quantity, int reservedQuantity)
		{
			Quantity = quantity;
			ReservedQuantity = reservedQuantity;
		}
	}
}

