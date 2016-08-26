using Topshelf;

namespace InventoryService.ServiceClientDeployment
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

                x.SetDescription("Inventory Sample Client");        //7
                x.SetDisplayName("Inventory Client");                       //8
                x.SetServiceName("InventoryClient");                       //9
            });                                                  //10
        }
    }
}
