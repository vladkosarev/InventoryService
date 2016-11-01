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
        public readonly ILoggingAdapter Logger = Context.GetLogger();

        public ProductInventoryActor(IInventoryStorage inventoryStorage, string id, bool withCache)
        {
            _id = id;
            _withCache = withCache;
            InventoryStorage = inventoryStorage;
            RealTimeInventory = RealTimeInventory.InitializeFromStorage(InventoryStorage, id);

            ReceiveAsync<GetInventoryMessage>(async message =>
            {
                if (_withCache == false)
                {
                    var result = await RealTimeInventory.ReadInventoryFromStorageAsync(InventoryStorage, message.ProductId);
                    RealTimeInventory = result.ProcessAndSendResult(message, CompletedMessageFactory.GetSuccessResponseCompletedMessage(message), Logger, RealTimeInventory, Sender).RealTimeInventory;
                }
                else
                {
                    RealTimeInventory = RealTimeInventory.ToSuccessOperationResult().ProcessAndSendResult(message, (rti) => new GetInventoryCompletedMessage(rti, true), Logger, RealTimeInventory, Sender).RealTimeInventory;
                }
            });

            ReceiveAsync<ReserveMessage>(async message =>
            {
                var result = await RealTimeInventory.ReserveAsync(InventoryStorage, message.ProductId, message.Update);
                RealTimeInventory = result.ProcessAndSendResult(message, CompletedMessageFactory.GetSuccessResponseCompletedMessage(message), Logger, RealTimeInventory, Sender).RealTimeInventory;
            });

            ReceiveAsync<UpdateQuantityMessage>(async message =>
            {
                var result = await RealTimeInventory.UpdateQuantityAsync(InventoryStorage, message.ProductId, message.Update);
                RealTimeInventory = result.ProcessAndSendResult(message, CompletedMessageFactory.GetSuccessResponseCompletedMessage(message), Logger, RealTimeInventory, Sender).RealTimeInventory;
            });

            ReceiveAsync<UpdateAndHoldQuantityMessage>(async message =>
            {
                var updateandHoldResultesult = await RealTimeInventory.UpdateQuantityAndHoldAsync(InventoryStorage, message.ProductId, message.Update);
                RealTimeInventory = updateandHoldResultesult.ProcessAndSendResult(message, CompletedMessageFactory.GetSuccessResponseCompletedMessage(message), Logger, RealTimeInventory, Sender).RealTimeInventory;
            });

            ReceiveAsync<PlaceHoldMessage>(async message =>
            {
                var result = await RealTimeInventory.PlaceHoldAsync(InventoryStorage, message.ProductId, message.Update);
                RealTimeInventory = result.ProcessAndSendResult(message, CompletedMessageFactory.GetSuccessResponseCompletedMessage(message), Logger, RealTimeInventory, Sender).RealTimeInventory;
            });

            ReceiveAsync<PurchaseMessage>(async message =>
            {
                var result = await RealTimeInventory.PurchaseAsync(InventoryStorage, message.ProductId, message.Update);
                RealTimeInventory = result.ProcessAndSendResult(message, CompletedMessageFactory.GetSuccessResponseCompletedMessage(message), Logger, RealTimeInventory, Sender).RealTimeInventory;
            });

            ReceiveAsync<PurchaseFromHoldsMessage>(async message =>
            {
                var result = await RealTimeInventory.PurchaseFromHoldsAsync(InventoryStorage, message.ProductId, message.Update).ConfigureAwait(false);
                RealTimeInventory = result.ProcessAndSendResult(message, CompletedMessageFactory.GetSuccessResponseCompletedMessage(message), Logger, RealTimeInventory, Sender).RealTimeInventory;
            });

            ReceiveAsync<FlushStreamsMessage>(async message =>
            {
                var result = await RealTimeInventory.InventoryStorageFlushAsync(InventoryStorage, _id);
                Sender.Tell(result.Data);
            });

            ReceiveAsync<ResetInventoryQuantityReserveAndHoldMessage>(async message =>
            {
                var updateandHoldResultesult = await RealTimeInventory.ResetInventoryQuantityReserveAndHoldAsync(InventoryStorage, message.ProductId, message.Update,message.Reservations,message.Holds);
                RealTimeInventory = updateandHoldResultesult.ProcessAndSendResult(message, CompletedMessageFactory.GetSuccessResponseCompletedMessage(message), Logger, RealTimeInventory, Sender).RealTimeInventory;
            });

#if DEBUG
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5), Nobody.Instance, RealTimeInventory, Self);
#endif
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