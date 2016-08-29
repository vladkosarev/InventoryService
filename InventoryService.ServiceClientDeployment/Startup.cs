using InventoryService.Console;
using Owin;

namespace InventoryService.ServiceClientDeployment
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            new SampleClientClass().StartSampleClientAsync();
        }
    }
}