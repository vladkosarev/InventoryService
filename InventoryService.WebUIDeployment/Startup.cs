using System;
using System.IO;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace InventoryService.WebUIDeployment
{
    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/web"))
            {
                appBuilder.MapSignalR();

                var fileSystem = new PhysicalFileSystem(AppDomain.CurrentDomain.BaseDirectory + "/web");
                var options = new FileServerOptions
                {
                    EnableDirectoryBrowsing = true,
                    FileSystem = fileSystem,
                    EnableDefaultFiles = true
                };

                appBuilder.UseFileServer(options);
            }

            //  InventoryServiceSignalRContext.Push();
        }
    }
}