namespace InventoryService.Messages
{
	public class WroteInventoryMessage
	{
		public WroteInventoryMessage(bool successful)
		{
			Successful = successful;
		}

		public bool Successful { get; private set; }
	}
}
