namespace InventoryService.Actors.Messages
{
    public class WroteReservationsMessage
    {
        public WroteReservationsMessage(bool successful)
        {
            Successful = successful;
        }

        public bool Successful { get; private set; }
    }
}