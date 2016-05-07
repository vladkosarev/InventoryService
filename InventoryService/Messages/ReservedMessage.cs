using System;

namespace InventoryService
{
	public class ReservedMessage
	{
		public string TicketSection { get; private set; }
		public int Quantity { get; private set; }
		public bool Successful { get; private set; }

		public ReservedMessage (string ticketSection, int quantity, bool successful)
		{
			TicketSection = ticketSection;
			Quantity = quantity;
			Successful = successful;
		}
	}
}

