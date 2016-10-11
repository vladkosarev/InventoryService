using System;
using Akka.Actor;
using Akka.Event;
using InventoryService.Actors.Messages;
using InventoryService.Messages;
using InventoryService.Messages.Models;
using InventoryService.Messages.Request;
using InventoryService.Messages.Response;
using InventoryService.Storage;

namespace InventoryService.Actors
{
    public class ProductInventoryActor : ReceiveActor, IWithUnboundedStash
    {
        private readonly string _id;
        private RealTimeInventory RealTimeInventory { set; get; }
        private readonly bool _withCache;
        private IInventoryStorage InventoryStorage { set; get; }
        public readonly ILoggingAdapter Logger = Context.GetLogger();

        public ProductInventoryActor(IInventoryStorage inventoryStorage, string id, bool withCache)
        {
          ReceiveAny(message =>
          {
              Stash.Stash();
          });
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
                    RealTimeInventory = result.ProcessAndSendResult(message, (rti) => new GetInventoryCompletedMessage(rti, true), Logger, RealTimeInventory, Sender);
                }
                else
                {
                    RealTimeInventory = RealTimeInventory.ToSuccessOperationResult().ProcessAndSendResult(message, (rti) => new GetInventoryCompletedMessage(rti, true), Logger, RealTimeInventory, Sender);
                }
            });

            ReceiveAsync<ReserveMessage>(async message =>
            {
                var result = await RealTimeInventory.ReserveAsync(InventoryStorage, message.ProductId, message.Update);
                RealTimeInventory = result.ProcessAndSendResult(message, (rti) => new ReserveCompletedMessage(rti, true), Logger, RealTimeInventory, Sender);
            });

            ReceiveAsync<UpdateQuantityMessage>(async message =>
            {
                var result = await RealTimeInventory.UpdateQuantityAsync(InventoryStorage, message.ProductId, message.Update);
                RealTimeInventory = result.ProcessAndSendResult(message, (rti) => new UpdateQuantityCompletedMessage(rti, true), Logger, RealTimeInventory, Sender);
            });

            ReceiveAsync<UpdateAndHoldQuantityMessage>(async message =>
            {
                var updateandHoldResultesult = await RealTimeInventory.UpdateQuantityAndHoldAsync(InventoryStorage, message.ProductId, message.Update);
                RealTimeInventory = updateandHoldResultesult.ProcessAndSendResult(message, (rti) => new UpdateAndHoldQuantityCompletedMessage(rti, true), Logger, RealTimeInventory, Sender);
            });

            ReceiveAsync<PlaceHoldMessage>(async message =>
            {
                var result = await RealTimeInventory.PlaceHoldAsync(InventoryStorage, message.ProductId, message.Update);
                RealTimeInventory = result.ProcessAndSendResult(message, (rti) => new PlaceHoldCompletedMessage(rti, true), Logger, RealTimeInventory, Sender);
            });

            ReceiveAsync<PurchaseMessage>(async message =>
            {
                var result = await RealTimeInventory.PurchaseAsync(InventoryStorage, message.ProductId, message.Update);
                RealTimeInventory = result.ProcessAndSendResult(message, (rti) => new PurchaseCompletedMessage(rti, true), Logger, RealTimeInventory, Sender);
            });

            ReceiveAsync<PurchaseFromHoldsMessage>(async message =>
            {
                var result = await RealTimeInventory.PurchaseFromHoldsAsync(InventoryStorage, message.ProductId, message.Update).ConfigureAwait(false);
                RealTimeInventory = result.ProcessAndSendResult(message, (rti) => new PurchaseFromHoldsCompletedMessage(rti, true), Logger, RealTimeInventory, Sender);
            });

            ReceiveAsync<FlushStreamsMessage>(async message =>
            {
                var result = await RealTimeInventory.InventoryStorageFlushAsync(InventoryStorage, _id);
                Sender.Tell(result.Data);
            });

#if DEBUG
            Context.System.Scheduler.ScheduleTellRepeatedly(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(5),Nobody.Instance, RealTimeInventory, Self);
#endif

            Stash.UnstashAll();
        }

        protected override void PostStop()
        {
            Sender.Tell(new InventoryOperationErrorMessage(new RealTimeInventory(_id, 0, 0, 0), new RealTimeInventoryException()
            {
                ErrorMessage = "Actor " + _id + " has stopped"
            }));

            Context.Parent.Tell(new RemoveProductMessage(RealTimeInventory));
            base.PostStop();
        }

        public IStash Stash { get; set; }
    }
}