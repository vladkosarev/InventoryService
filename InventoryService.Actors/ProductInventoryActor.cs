using Akka.Actor;
using Akka.Event;
using InventoryService.Actors.Messages;
using InventoryService.Messages;
using InventoryService.Messages.Models;
using InventoryService.Messages.Request;
using InventoryService.Messages.Response;
using InventoryService.Storage;
using System;

namespace InventoryService.Actors
{
    public class ProductInventoryActor : ReceiveActor
    {
        private readonly string _id;
        private RealTimeInventory RealTimeInventory { set; get; }
        private readonly bool _withCache;
        private IInventoryStorage InventoryStorage { set; get; }
        public readonly ILoggingAdapter Logger = null;// Context.GetLogger();
        public IActorRef NotificationActorRef { get; set; }
        public ProductInventoryActor(IInventoryStorage inventoryStorage, IActorRef notificationsActorRef, string id, bool withCache, IPerformanceService performanceService)
        {
            PerformanceService = performanceService;
            _id = id;
            _withCache = withCache;
            InventoryStorage = inventoryStorage;
            RealTimeInventory = RealTimeInventory.InitializeFromStorage(InventoryStorage, id);
            NotificationActorRef = notificationsActorRef;
            ReceiveAsync<GetInventoryMessage>(async message =>
            {
               
                if (!CanProcessMessage(message.ProductId, message))
                {
                    return;
                }

                if (_withCache == false)
                {
                    var result = await RealTimeInventory.ReadInventoryFromStorageAsync(InventoryStorage, message.ProductId);
                    RealTimeInventory = result.ProcessAndSendResult(message, CompletedMessageFactory.GetResponseCompletedMessage(message), Logger, RealTimeInventory, Sender, NotificationActorRef, PerformanceService).RealTimeInventory;
                }
                else
                {
                    RealTimeInventory = RealTimeInventory.ToSuccessOperationResult().ProcessAndSendResult(message, (rti) => new GetInventoryCompletedMessage(rti, true), Logger, RealTimeInventory, Sender, NotificationActorRef, PerformanceService).RealTimeInventory;
                }

            });

            ReceiveAsync<ReserveMessage>(async message =>
            {
                if (!CanProcessMessage(message.ProductId, message))
                {
                    return;
                }
                var result = await RealTimeInventory.ReserveAsync(InventoryStorage, message.ProductId, message.Update);
                RealTimeInventory = result.ProcessAndSendResult(message, CompletedMessageFactory.GetResponseCompletedMessage(message), Logger, RealTimeInventory, Sender, NotificationActorRef, PerformanceService).RealTimeInventory;
              
            });

            ReceiveAsync<UpdateQuantityMessage>(async message =>
            {
                if (!CanProcessMessage(message.ProductId, message))
                {
                    return;
                }
                var result = await RealTimeInventory.UpdateQuantityAsync(InventoryStorage, message.ProductId, message.Update);
                RealTimeInventory = result.ProcessAndSendResult(message, CompletedMessageFactory.GetResponseCompletedMessage(message), Logger, RealTimeInventory, Sender, NotificationActorRef, PerformanceService).RealTimeInventory;
            });

            ReceiveAsync<UpdateAndHoldQuantityMessage>(async message =>
            {
                if (!CanProcessMessage(message.ProductId, message))
                {
                    return;
                }
                var updateandHoldResultesult = await RealTimeInventory.UpdateQuantityAndHoldAsync(InventoryStorage, message.ProductId, message.Update);
                RealTimeInventory = updateandHoldResultesult.ProcessAndSendResult(message, CompletedMessageFactory.GetResponseCompletedMessage(message), Logger, RealTimeInventory, Sender, NotificationActorRef, PerformanceService).RealTimeInventory;
            });

            ReceiveAsync<PlaceHoldMessage>(async message =>
            {
                if (!CanProcessMessage(message.ProductId, message))
                {
                    return;
                }
                var result = await RealTimeInventory.PlaceHoldAsync(InventoryStorage, message.ProductId, message.Update);
                RealTimeInventory = result.ProcessAndSendResult(message, CompletedMessageFactory.GetResponseCompletedMessage(message), Logger, RealTimeInventory, Sender, NotificationActorRef, PerformanceService).RealTimeInventory;
            });

            ReceiveAsync<PurchaseMessage>(async message =>
            {
                if (!CanProcessMessage(message.ProductId, message))
                {
                    return;
                }
                var result = await RealTimeInventory.PurchaseAsync(InventoryStorage, message.ProductId, message.Update);
                RealTimeInventory = result.ProcessAndSendResult(message, CompletedMessageFactory.GetResponseCompletedMessage(message), Logger, RealTimeInventory, Sender, NotificationActorRef, PerformanceService).RealTimeInventory;
            });

            ReceiveAsync<PurchaseFromHoldsMessage>(async message =>
            {
                if (!CanProcessMessage(message.ProductId, message))
                {
                    return;
                }
                var result = await RealTimeInventory.PurchaseFromHoldsAsync(InventoryStorage, message.ProductId, message.Update).ConfigureAwait(false);
                RealTimeInventory = result.ProcessAndSendResult(message, CompletedMessageFactory.GetResponseCompletedMessage(message), Logger, RealTimeInventory, Sender, NotificationActorRef, PerformanceService).RealTimeInventory;
            });

            ReceiveAsync<FlushStreamsMessage>(async message =>
            {
                var result = await RealTimeInventory.InventoryStorageFlushAsync(InventoryStorage, _id);
                Sender.Tell(result.Data);
            });

            ReceiveAsync<ResetInventoryQuantityReserveAndHoldMessage>(async message =>
            {
                if (!CanProcessMessage(message.ProductId, message))
                {
                    return;
                }
                var updateandHoldResultesult = await RealTimeInventory.ResetInventoryQuantityReserveAndHoldAsync(InventoryStorage, message.ProductId, message.Update, message.Reservations, message.Holds);
                RealTimeInventory = updateandHoldResultesult.ProcessAndSendResult(message, CompletedMessageFactory.GetResponseCompletedMessage(message), Logger, RealTimeInventory, Sender, NotificationActorRef, PerformanceService).RealTimeInventory;
            });

#if DEBUG
//            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5), Nobody.Instance, RealTimeInventory, Self);
#endif
        }

        public IPerformanceService PerformanceService { get; set; }

        private bool CanProcessMessage(string productId,IRequestMessage message)
        {
            if (_id == productId) return true;

            var errorMessage = "Invalid request made to " + nameof(ProductInventoryActor) + " with an id of " +productId + " but my Id is " + _id + ". Message will not be processed ";
            Logger.Error(errorMessage);
            new OperationResult<IRealTimeInventory>()
            {
                IsSuccessful = false,
                Exception = new RealTimeInventoryException()
                {
                    ErrorMessage = errorMessage
                }
            }.ProcessAndSendResult(message, CompletedMessageFactory.GetResponseCompletedMessage(message,false), Logger,RealTimeInventory, Sender, NotificationActorRef, PerformanceService);
            NotificationActorRef.Tell(errorMessage);
            return false;
        }

        protected override void PostStop()
        {
            Logger.Error(" I , the " + _id + " have been stopped. At the moment my inventory is " + RealTimeInventory.GetCurrentQuantitiesReport());
            Sender.Tell(new InventoryOperationErrorMessage(new RealTimeInventory(_id, 0, 0, 0), new RealTimeInventoryException()
            {
                ErrorMessage = "Actor " + _id + " has stopped"
            }));

            Context.Parent.Tell(new RemoveProductMessage(RealTimeInventory));
            base.PostStop();
        }
    }
}