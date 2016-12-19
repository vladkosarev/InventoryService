using Akka.Actor;
using Akka.Event;
using InventoryService.ActorMailBoxes;
using InventoryService.Messages;
using InventoryService.Messages.Request;
using InventoryService.Messages.Response;
using InventoryService.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using InventoryService.BackUpService;

namespace InventoryService.Actors
{
    public class InventoryActor : ReceiveActor
    {
        private readonly Dictionary<string, IActorRef> _products = new Dictionary<string, IActorRef>();
        private readonly Dictionary<string, RemoveProductMessage> _removedRealTimeInventories = new Dictionary<string, RemoveProductMessage>();
        public readonly ILoggingAdapter Logger = Context.GetLogger();
        private IInventoryStorage InventoryStorage { set; get; }
        private readonly bool _withCache;
        public IActorRef NotificationActorRef { get; set; }
        public IActorRef InventoryServicePingActor { set; get; }
        private IBackUpService BackUpService { set; get; }
        public InventoryActor(IInventoryStorage inventoryStorage, IPerformanceService performanceService,IBackUpService backUpService, bool withCache = true)
        {
            if (backUpService == null) throw new ArgumentNullException(nameof(backUpService));
            BackUpService = backUpService;
            PerformanceService = performanceService;
            PerformanceService.Init();
            Logger.Debug("Starting Inventory Actor ....");
            InventoryStorage = inventoryStorage;
            _withCache = withCache;
            Become(Initializing);
        }

        public IPerformanceService PerformanceService { get; set; }

        private void Initializing()
        {
            Logger.Debug("Inventory Actor Initializing ....");
            Self.Tell(new InitializeInventoriesFromStorageMessage());
            ReceiveAsync<InitializeInventoriesFromStorageMessage>(async message =>
            {
                var inventoryIdsResult = await InventoryStorage.ReadAllInventoryIdAsync();

                if (inventoryIdsResult.IsSuccessful)
                {
                    Become(Processing);
                    foreach (var s in inventoryIdsResult.Result)
                    {
                        Logger.Debug("Initializing asking " + s + " for its inventory ....");
                        var invActorRef = GetActorRef(InventoryStorage, s, PerformanceService);
                        invActorRef.Tell(new GetInventoryMessage(s));
                    }
                }
                else
                {
                    var errorMsg = "Failed to read inventories from storage " + InventoryStorage.GetType().FullName + " - " + inventoryIdsResult.Errors.Flatten().Message;
                    Logger.Error("Inventory Actor Initialization Failed " + errorMsg);
                    throw new Exception(errorMsg, inventoryIdsResult.Errors.Flatten());
                }
            });
        }

        private void Processing()
        {
            Logger.Debug("Inventory Actor Processing started ...");

            NotificationActorRef = Context.ActorOf(Props.Create(() => new InventoryQueryActor(BackUpService)).WithMailbox(nameof(GetAllInventoryListMailbox)), typeof(InventoryQueryActor).Name);
            InventoryServicePingActor = Context.ActorOf(Props.Create(() => new InventoryServicePingActor()), typeof(InventoryServicePingActor).Name);

            Receive<RemoveProductMessage>(message =>
            {
                var productId = message?.RealTimeInventory?.ProductId;

                Logger.Debug("Actor " + productId + " has requested to be removed because " + message?.Reason?.Message + " and so will no longer be sent messages.", message);

                if (!string.IsNullOrEmpty(productId))
                {
                    _products.Remove(productId);
                    _removedRealTimeInventories[productId] = message;
                    NotificationActorRef.Tell(productId + " Actor has died! Reason : " + message?.Reason?.Message);
                }
            });

            Receive<GetRemovedProductMessage>(message =>
            {
                Sender.Tell(new GetRemovedProductCompletedMessage(_removedRealTimeInventories.Select(x => x.Value).ToList()));
            });

            Receive<QueryInventoryListMessage>(message =>
            {
                foreach (var product in _products)
                    product.Value.Tell(new GetInventoryMessage(product.Key));
            });

            Receive<GetInventoryCompletedMessage>(message =>
            {
                //todo remove this
                // _realTimeInventories[message.RealTimeInventory.ProductId] = message.RealTimeInventory;
                // NotificationActorRef.Tell(new QueryInventoryListCompletedMessage(new List<IRealTimeInventory>() { message.RealTimeInventory }));
            });

            Receive<IRequestMessage>(message =>
            {
                PerformanceService.Increment("Incoming Message");
                Logger.Debug(message.GetType().Name + " received for " + message.ProductId + " for update " + message.Update);
                var actorRef = GetActorRef(InventoryStorage, message.ProductId, PerformanceService);
                actorRef.Forward(message);
            });
            Receive<GetMetrics>(_ =>
            {
                PerformanceService.PrintMetrics();
            });
#if DEBUG
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.Zero, TimeSpan.FromMilliseconds(1000), Self, new GetMetrics(), Nobody.Instance);
#endif
        }

        private IActorRef GetActorRef(IInventoryStorage inventoryStorage, string productId, IPerformanceService performanceService)
        {
            if (_products.ContainsKey(productId)) return _products[productId];

            Logger.Debug("Creating inventory actor " + productId + " since it does not yet exist ...");
            var productActorRef = Context.ActorOf(
                Props.Create(() =>
                    new ProductInventoryActor(inventoryStorage, NotificationActorRef, productId, _withCache, performanceService))
                , productId);

            _products.Add(productId, productActorRef);
            return _products[productId];
        }

        protected override SupervisorStrategy SupervisorStrategy()
        {
            return new OneForOneStrategy(
                x =>
                {
                    var message = x.Message + " - " + x.InnerException?.Message + " - it's possible an inventory actor has mal-functioned so i'm going to stop it :( ";
                    Logger.Error(message);
                    NotificationActorRef.Tell(message);
                    return Directive.Restart;
                });
        }
    }
}