using System;

namespace InventoryService
{
	public class ReserveMessage
	{
		public string TicketSection { get; private set; }
		public int Quantity { get; private set; }

		public ReserveMessage (string ticketSection, int quantity)
		{
			TicketSection = ticketSection;
			Quantity = quantity;
		}
	}
}

