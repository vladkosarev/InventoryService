using InventoryService.NullBackUpService;
using NLog;

namespace InventoryService.Server
{
    internal class Program
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            new InventoryServiceApplication().Start(new TestPerformanceService(), new NullBackUp(), (a, s) =>
                {
                    Log.Debug("Server started ...");
                }
            );
        }
    }
}