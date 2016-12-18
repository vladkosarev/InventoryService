namespace InventoryService.Messages
{
    public class ExportAllInventoryCompletedMessage
    {
        public ExportAllInventoryCompletedMessage(string inventoriesCsv)
        {
            InventoriesCsv = inventoriesCsv;
        }

        public string InventoriesCsv { get; }
    }
}