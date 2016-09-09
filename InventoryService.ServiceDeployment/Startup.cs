using InventoryService.Server;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;
using System;

namespace InventoryService.ServiceDeployment
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            new InventoryServiceServer().StartServer();
            appBuilder.MapSignalR();

            var fileSystem = new PhysicalFileSystem(AppDomain.CurrentDomain.BaseDirectory + "/web");
            var options = new FileServerOptions
            {
                EnableDirectoryBrowsing = true,
                FileSystem = fileSystem,
                EnableDefaultFiles = true
            };

            appBuilder.UseFileServer(options);

          //  InventoryServiceSignalRContext.Push();
        }
    }
}