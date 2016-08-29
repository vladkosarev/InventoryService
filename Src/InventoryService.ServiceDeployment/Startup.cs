using InventoryService.Server;
using Owin;

namespace InventoryService.ServiceDeployment
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            new InventoryServiceServer().StartServer();
        }
 
    }

}