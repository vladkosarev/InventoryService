using InventoryService.Console;
using Owin;

namespace InventoryService.ServiceClientDeployment
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            System.Threading.Thread.Sleep(5000);
            new SampleClientClass().StartSampleClientAsync();
        }
    }
}