using NLog;

namespace InventoryService.Server
{
    internal class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            new InventoryServiceApplication().Start((a, s) =>
            {
                Log.Debug("Server started ...");
            }
            );
        }
    }
}