using System;
using InventoryService.AzureBlobBackUpService;
using InventoryService.FileSystemBackUpService;
using InventoryService.Server;
using Topshelf;

namespace InventoryService.ServiceDeployment
{
    public class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                HostFactory.Run(x =>                                 //1
                {
                    x.Service<InventoryServiceApplication>(s =>                        //2
                    {
                        s.ConstructUsing(name => new InventoryServiceApplication());     //3
                        s.WhenStarted(tc => tc.Start(new TestPerformanceService(), new AzureBlobBackUp()));              //4
                        s.WhenStopped(tc => tc.Stop());               //5
                    });
                    x.RunAsLocalSystem();                            //6
                    x.UseNLog();
                    x.SetDescription("Inventory MicroService");        //7
                    x.SetDisplayName("Inventory Service");                       //8
                    x.SetServiceName("InventoryService");                   //9
                });
            }
            catch (Exception t)
            {
                
                throw;
            }                                         //10
        }
    }
}