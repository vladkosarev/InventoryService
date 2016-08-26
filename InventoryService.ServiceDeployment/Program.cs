using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using InventoryService.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Topshelf;

namespace InventoryService.ServiceDeployment
{
  public  class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>                                 //1
            {
                x.Service<MyServiceClass>(s =>                        //2
                {
                    s.ConstructUsing(name => new MyServiceClass());     //3
                    s.WhenStarted(tc => tc.Start());              //4
                    s.WhenStopped(tc => tc.Stop());               //5
                });
                x.RunAsLocalSystem();                            //6

                x.SetDescription("Inventory MicroService");        //7
                x.SetDisplayName("Inventory Service");                       //8
                x.SetServiceName("InventoryService");                       //9
            });                                                  //10
        }
    }
}
