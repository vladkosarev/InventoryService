using Akka.Actor;
using Microsoft.Owin.Hosting;
using System;
using System.Configuration;
using InventoryService.Diagnostics;
using NLog;

namespace InventoryService.Server
{
    public class InventoryServiceApplication
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void Start(Action<IActorRef, ActorSystem> onReady = null,
            Type storageType = null
            , string serverEndPoint = null
            , string serverActorSystemName = null
            , ActorSystem serverActorSystem = null
            , string serverActorSystemConfig = null
        )
        {
            try
            {
                Log.Debug("Starting inventory service ...");
                serverEndPoint = serverEndPoint ?? ConfigurationManager.AppSettings["ServerEndPoint"];

                if (!string.IsNullOrEmpty(serverEndPoint))
                {
                    // Start OWIN host
                    OwinRef = WebApp.Start<Startup>(url: serverEndPoint);
                }

                InventoryServiceServerApp = new InventoryServiceServerApp();
                    InventoryServiceServerApp.StartServer(onReady, storageType, serverActorSystemName: serverActorSystemName, serverActorSystem: serverActorSystem, serverActorSystemConfig: serverActorSystemConfig);
          
            }
            catch (Exception e)
            {
                Log.Error(e,"Unable to start inventory service");
                throw;
            }
            //
        }

        public InventoryServiceServerApp InventoryServiceServerApp { get; set; }

        protected IDisposable OwinRef { get; set; }

        public void Stop()
        {
          //  InventoryServiceServerApp.StopServer();
            OwinRef?.Dispose();
        }
    }
}