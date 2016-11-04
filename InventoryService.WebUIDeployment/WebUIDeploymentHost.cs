using Akka.Actor;
using InventoryService.ActorSystemFactoryLib;

using Microsoft.Owin.Hosting;
using NLog;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using Autofac;
using Autofac.Integration.SignalR;
using InventoryService.WebUIHost;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

namespace InventoryService.WebUIDeployment
{
    public class WebUIDeploymentHost
    {
        public WebUIDeploymentHost()
        {
            ActorSystemFactory = new ActorSystemFactory();
        }

        private ActorSystemFactory ActorSystemFactory { set; get; }
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void Start(string serverEndPoint = null, string serverActorSystemName = null,
            ActorSystem serverActorSystem = null, string serverActorSystemConfig = null)
        {
            try
            {
                ActorSystemFactory.CreateOrSetUpActorSystem(serverActorSystemName: serverActorSystemName,actorSystem: serverActorSystem,actorSystemConfig: serverActorSystemConfig);
                var inventoryActorAddress = ConfigurationManager.AppSettings["RemoteActorAddress"];
                var signalRNotificationsActorRef = ActorSystemFactory.InventoryServiceActorSystem.ActorOf(Props.Create(() => new SignalRNotificationsActor(inventoryActorAddress)), typeof(SignalRNotificationsActor).Name);

                const string message = "signalRNotificationsActor created !!!!";
                Log.Debug(message);
                Console.WriteLine(message);

                Log.Debug("Starting inventory service ...");
                serverEndPoint = serverEndPoint ?? ConfigurationManager.AppSettings["ServerEndPoint"];

                if (!string.IsNullOrEmpty(serverEndPoint))
                {
                    OwinRef = WebApp.Start( serverEndPoint, (appBuilder) =>
                    {
                        if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/web"))
                        {

                            var builder = new ContainerBuilder();
                            builder.Register(c=> signalRNotificationsActorRef).SingleInstance();
                            // Register your SignalR hubs.
                            builder.RegisterHubs(Assembly.GetExecutingAssembly());

                            var container = builder.Build();

                            appBuilder.UseAutofacMiddleware(container);

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

                    });
                }

                Log.Debug("WebUIDeployment initialized successfully");
    
            }
            catch (Exception e)
            {
                Log.Error(e, "Unable to start inventory service UI");
                throw;
            }
        }

        protected IDisposable OwinRef { get; set; }

        public void Stop()
        {
            OwinRef?.Dispose();
        }
    }
}