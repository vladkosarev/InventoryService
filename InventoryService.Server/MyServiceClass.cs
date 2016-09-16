using Microsoft.Owin.Hosting;
using System;
using System.Configuration;

namespace InventoryService.Server
{
    public class InventoryServiceApplication
    {
        public void Start()
        {
            var address = ConfigurationManager.AppSettings["ServerEndPoint"];
            // Start OWIN host
            OwinRef = WebApp.Start<Startup>(url: address);
            InventoryServiceServerApp = new InventoryServiceServerApp();
            InventoryServiceServerApp.StartServer();
            // Console.ReadLine();
        }

        public InventoryServiceServerApp InventoryServiceServerApp { get; set; }

        public IDisposable OwinRef { get; set; }

        public void Stop()
        {
            InventoryServiceServerApp.StopServer();
            OwinRef?.Dispose();
        }
    }
}