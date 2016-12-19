using Akka.Actor;
using NLog;
using System;
using InventoryService.BackUpService;

namespace InventoryService.Server
{
    public class InventoryServiceApplication
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void Start(IPerformanceService performanceService, IBackUpService backUpService, Action<IActorRef, ActorSystem> onReady = null,
            Type storageType = null
            , string serverEndPoint = null
            , string serverActorSystemName = null
            , ActorSystem serverActorSystem = null
            , string serverActorSystemConfig = null
        )
        {
            try
            {
                InventoryServiceServerApp = new InventoryServiceServerApp();
                InventoryServiceServerApp.StartServer(performanceService, backUpService, onReady, storageType, serverActorSystemName: serverActorSystemName, serverActorSystem: serverActorSystem, serverActorSystemConfig: serverActorSystemConfig);
            }
            catch (Exception e)
            {
                Log.Error(e, "Unable to start inventory service");
                throw;
            }
            //
        }

        public InventoryServiceServerApp InventoryServiceServerApp { get; set; }

        public void Stop()
        {
            InventoryServiceServerApp.StopServer();
        }
    }
}