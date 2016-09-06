using Akka.Actor;
using Akka.Event;
using InventoryService.Messages;
using InventoryService.Messages.Models;
using InventoryService.Messages.Request;
using InventoryService.Messages.Response;
using InventoryService.Services;
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
            Become(Running);
        }

        private void Running()
        {
            ReceiveAsync<GetInventoryMessage>(async message =>
            {
                if (_withCache == false)
                {
                    var result = await RealTimeInventory.ReadInventoryFromStorageAsync(InventoryStorage, message.ProductId);
                    ProcessAndSendResult(result, message, (rti) => new GetInventoryCompletedMessage(rti, true));
                }
                ProcessAndSendResult(RealTimeInventory.ToSuccessOperationResult(), message, (rti) => new GetInventoryCompletedMessage(rti, true));
            });

            ReceiveAsync<ReserveMessage>(async message =>
            {
                //throw  new Exception();

                var result = await RealTimeInventory.ReserveAsync(InventoryStorage, message.ProductId, message.Update);
                ProcessAndSendResult(result, message, (rti) => new ReserveCompletedMessage(rti, true));
            });

            ReceiveAsync<UpdateQuantityMessage>(async message =>
            {
                var result = await RealTimeInventory.UpdateQuantityAsync(InventoryStorage, message.ProductId, message.Update);
                ProcessAndSendResult(result, message, (rti) => new UpdateQuantityCompletedMessage(rti, true));
            });

            ReceiveAsync<UpdateAndHoldQuantityMessage>(async message =>
            {
                var updateandHoldResultesult = await RealTimeInventory.UpdateQuantityAndHoldAsync(InventoryStorage, message.ProductId, message.Update);
                ProcessAndSendResult(updateandHoldResultesult, message, (rti) => new PlaceHoldCompletedMessage(rti, true));
            });

            ReceiveAsync<PlaceHoldMessage>(async message =>
            {
                var result = await RealTimeInventory.PlaceHoldAsync(InventoryStorage, message.ProductId, message.Update);
                ProcessAndSendResult(result, message, (rti) => new PlaceHoldCompletedMessage(rti, true));
            });

            ReceiveAsync<PurchaseMessage>(async message =>
            {
                var result = await RealTimeInventory.PurchaseAsync(InventoryStorage, message.ProductId, message.Update);
                ProcessAndSendResult(result, message, (rti) => new PurchaseCompletedMessage(rti, true));
            });

            ReceiveAsync<PurchaseFromHoldsMessage>(async message =>
            {
                var result = await RealTimeInventory.PurchaseFromHoldsAsync(InventoryStorage, message.ProductId, message.Update).ConfigureAwait(false);
                ProcessAndSendResult(result, message, (rti) => new PurchaseFromHoldsCompletedMessage(rti, true));
            });

            ReceiveAsync<FlushStreamsMessage>(async message =>
            {
                var result = await RealTimeInventory.InventoryStorageFlushAsync(InventoryStorage, _id);
                Sender.Tell(result.Data);
            });
        }

        private void ProcessAndSendResult(OperationResult<IRealTimeInventory> result, IRequestMessage requestMessage, Func<RealTimeInventory, IInventoryServiceCompletedMessage> successResponseCompletedMessage)
        {
            Logger.Info(requestMessage.GetType().Name + " Request was " + (!result.IsSuccessful ? " NOT " : "") + " successful.  Current Inventory :  " + RealTimeInventory.GetCurrentQuantitiesReport());
            if (!result.IsSuccessful)
            {
                Sender.Tell(result.ToInventoryOperationErrorMessage(requestMessage.ProductId));
                Logger.Error(result.Exception.Message);
            }
            else
            {
                RealTimeInventory = result.Data as RealTimeInventory;
                var response = successResponseCompletedMessage(RealTimeInventory);
                Sender.Tell(response);
                Logger.Info(response.GetType().Name + " Response was sent back. Current Inventory : " + RealTimeInventory.GetCurrentQuantitiesReport());
            }
        }

        protected override void PostStop()
        {
            Sender.Tell(new InventoryOperationErrorMessage(new RealTimeInventory(_id, 0, 0, 0), new Exception("oh oh")));

            Context.Parent.Tell(new RemoveProductMessage(RealTimeInventory));

            base.PostStop();
        }
    }
}