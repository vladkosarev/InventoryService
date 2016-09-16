using Microsoft.Owin.Hosting;
using System;
using System.Configuration;
using Akka.Actor;

namespace InventoryService.Server
{
    public class InventoryServiceApplication
    {
        public void Start(
            Type storageType=null
            , string serverEndPoint = null
            , string serverActorSystemName = null
            , ActorSystem serverActorSystem = null
            ,string serverActorSystemConfig=null
           )
        {
          var  tmpServerEndPoint = serverEndPoint ?? ConfigurationManager.AppSettings["ServerEndPoint"];

            serverEndPoint = tmpServerEndPoint ?? serverEndPoint;
           // Start OWIN host
            OwinRef = WebApp.Start<Startup>(url: serverEndPoint);
            InventoryServiceServerApp = new InventoryServiceServerApp();
            InventoryServiceServerApp.StartServer(storageType,serverActorSystemName: serverActorSystemName, serverActorSystem: serverActorSystem, serverActorSystemConfig: serverActorSystemConfig);
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