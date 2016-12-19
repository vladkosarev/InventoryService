using Akka.Actor;
using Autofac;
using Autofac.Integration.SignalR;
using InventoryService.ActorMailBoxes;
using InventoryService.ActorSystemFactoryLib;
using InventoryService.WebUIHost;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Hosting;
using Microsoft.Owin.StaticFiles;
using NLog;
using Owin;
using System;
using System.Configuration;
using System.IO;

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
                ActorSystemFactory.CreateOrSetUpActorSystem(serverActorSystemName: serverActorSystemName, actorSystem: serverActorSystem, actorSystemConfig: serverActorSystemConfig);
                var inventoryNotificationActorAddress = ConfigurationManager.AppSettings["RemoteInventoryInventoryQueryActorAddress"];
                var RemoteInventoryActorAddress = ConfigurationManager.AppSettings["RemoteInventoryActorAddress"];
                var signalRInventoryQueryActorRef = ActorSystemFactory.InventoryServiceActorSystem.ActorOf(Props.Create(() => new SignalRInventoryQueryActor(inventoryNotificationActorAddress, RemoteInventoryActorAddress)).WithMailbox(nameof(GetAllInventoryListMailbox)), typeof(SignalRInventoryQueryActor).Name);

                const string message = "signalRInventoryQueryActor created !!!!";
                Log.Debug(message);
                Console.WriteLine(message);

                Log.Debug("Starting inventory service ...");
                serverEndPoint = serverEndPoint ?? ConfigurationManager.AppSettings["ServerEndPoint"];

                if (!string.IsNullOrEmpty(serverEndPoint))
                {
                    OwinRef = WebApp.Start(serverEndPoint, (appBuilder) =>
                   {
                       if (Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + "/web"))
                       {
                           var builder = new ContainerBuilder();
                           builder.Register(c => signalRInventoryQueryActorRef).ExternallyOwned();
                           // Register your SignalR hubs.
                           builder.RegisterType<InventoryServiceHub>().ExternallyOwned();

                           var container = builder.Build();
                           //var config = new HubConfiguration {Resolver = new AutofacDependencyResolver(container)};
                           GlobalHost.DependencyResolver = new AutofacDependencyResolver(container);
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