using System;

namespace InventoryService
{
	public class LoadedInventoryMessage {
		public int Quantity { get; private set; }
		public int Reservations { get; private set; }

		public LoadedInventoryMessage (int quantity, int reservations)
		{
			Quantity = quantity;
			Reservations = reservations;
		}
	}
}

