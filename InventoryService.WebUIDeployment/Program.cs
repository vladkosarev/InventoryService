using Topshelf;

namespace InventoryService.WebUIDeployment
{
    public class Program
    {
        private static void Main(string[] args)
        {
            HostFactory.Run(x =>                                 //1
            {
                x.Service<WebUIDeploymentHost>(s =>                        //2
                {
                    s.ConstructUsing(name => new WebUIDeploymentHost());     //3
                    s.WhenStarted(tc => tc.Start());              //4
                    s.WhenStopped(tc => tc.Stop());               //5
                });
                x.RunAsLocalSystem();                            //6
                x.UseNLog();
                x.SetDescription("Inventory MicroService UI");        //7
                x.SetDisplayName("Inventory Service UI");                       //8
                x.SetServiceName("InventoryService UI");                   //9
            });                                                  //10

        }
    }
}