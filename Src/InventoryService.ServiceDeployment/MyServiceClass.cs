using System;
using System.Configuration;
using Microsoft.Owin.Hosting;

namespace InventoryService.ServiceDeployment
{
    public class MyServiceClass
    {
        public void Start()
        {
            var address = ConfigurationManager.AppSettings["ServerEndPoint"]; 
            // Start OWIN host
            using (WebApp.Start<Startup>(url: address))
            {
                Console.WriteLine("Server started ...");
                Console.ReadKey();
            }
        }

        public void Stop()
        {

        }
    }
}