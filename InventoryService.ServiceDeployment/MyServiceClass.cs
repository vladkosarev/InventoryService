using Microsoft.Owin.Hosting;
using System;
using System.Configuration;

namespace InventoryService.ServiceDeployment
{
    public class MyServiceClass
    {
        public void Start()
        {
            var address = ConfigurationManager.AppSettings["ServerEndPoint"];
            // Start OWIN host
            OwinRef = WebApp.Start<Startup>(url: address);
            Console.WriteLine("Server started ...");
            Console.ReadKey();
        }

        public IDisposable OwinRef { get; set; }

        public void Stop()
        {
            OwinRef?.Dispose();
        }
    }
}