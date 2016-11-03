using Microsoft.AspNet.SignalR;
using NLog;

namespace InventoryService.WebUIHost
{
    public class InventoryServiceHub : Hub
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void GetInventoryList()
        {            
            Log.Debug("A client has joined the system and will be receiving messages");
        }
    }
}