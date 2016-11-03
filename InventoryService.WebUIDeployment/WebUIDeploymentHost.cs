using Akka.Actor;
using InventoryService.ActorSystemFactoryLib;
using InventoryService.WebUIHost;
using Microsoft.Owin.Hosting;
using NLog;
using System;
using System.Configuration;

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
                Log.Debug("Starting inventory service ...");
                serverEndPoint = serverEndPoint ?? ConfigurationManager.AppSettings["ServerEndPoint"];

                if (!string.IsNullOrEmpty(serverEndPoint))
                {
                    OwinRef = WebApp.Start<Startup>(url: serverEndPoint);
                }

                ActorSystemFactory.CreateOrSetUpActorSystem(
                 serverActorSystemName: serverActorSystemName,
                 actorSystem: serverActorSystem,
                 actorSystemConfig: serverActorSystemConfig);
                var inventoryActorAddress = ConfigurationManager.AppSettings["RemoteActorAddress"];
                var signalRNotificationsActor = ActorSystemFactory.InventoryServiceActorSystem.ActorOf(Props.Create(() => new SignalRNotificationsActor(inventoryActorAddress)), typeof(SignalRNotificationsActor).Name);

                const string message = "signalRNotificationsActor created !!!!";
                Log.Debug(message);
                Console.WriteLine(message);
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