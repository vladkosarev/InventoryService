using Akka.Actor;
using Microsoft.Owin.Hosting;
using System;
using System.Configuration;

namespace InventoryService.Server
{
    public class InventoryServiceApplication
    {
        public void Start(Action<IActorRef, ActorSystem> onReady = null,
            Type storageType = null
            , string serverEndPoint = null
            , string serverActorSystemName = null
            , ActorSystem serverActorSystem = null
            , string serverActorSystemConfig = null
           )
        {
            serverEndPoint = serverEndPoint ?? ConfigurationManager.AppSettings["ServerEndPoint"];

            // Start OWIN host
            OwinRef = WebApp.Start<Startup>(url: serverEndPoint);
            InventoryServiceServerApp = new InventoryServiceServerApp();
            InventoryServiceServerApp.StartServer(onReady, storageType, serverActorSystemName: serverActorSystemName, serverActorSystem: serverActorSystem, serverActorSystemConfig: serverActorSystemConfig);
            // Console.ReadLine();
        }

        public InventoryServiceServerApp InventoryServiceServerApp { get; set; }

        protected IDisposable OwinRef { get; set; }

        public void Stop()
        {
            InventoryServiceServerApp.StopServer();
            OwinRef?.Dispose();
        }
    }
}