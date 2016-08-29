using Akka.Actor;
using InventoryService.Messages;
using InventoryService.Storage;
using System;
using System.Threading.Tasks;
using InventoryService.Messages.Request;
using InventoryService.Messages.Response;
using InventoryService.Services;

namespace InventoryService.Actors
{
    public class ProductInventoryActor : ReceiveActor
    {
        private readonly string _id;
       

        private readonly bool _withCache;
        
        private readonly IProductInventoryOperations _productInventoryOperations;

        public ProductInventoryActor(IInventoryStorage inventoryStorage, string id, bool withCache)
        {
            _id = id;
          
            _withCache = withCache;
          

            Become(Running);
            _productInventoryOperations = new ProductInventoryOperations(inventoryStorage,id);

            //Context.System.Scheduler.ScheduleTellRepeatedly(
            //    TimeSpan.Zero
            //    , TimeSpan.FromMilliseconds(100)
            //    , Self
            //    , new FlushStreamMessage(_id)
            //    , ActorRefs.Nobody);
        }

        private void Running()
        {
            Receive<GetInventoryMessage>(message =>
           {
               if (!_withCache)
               {
                   _productInventoryOperations.ReadInventory(message.ProductId).ContinueWith(result =>
                   {
                       var quantity = result.Result.Quantity;
                       var reservations = result.Result.Reservations;
                       var holds = result.Result.Holds;
                       return new GetInventoryCompletedMessage(message.ProductId, quantity, reservations,holds);
                   },
                   TaskContinuationOptions.AttachedToParent
                   & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
               }
           });

            Receive<ReserveMessage>(message =>
           {
               _productInventoryOperations.Reserve(message.ProductId, message.ReservationQuantity).ContinueWith(result =>
               {
                   return new ReserveCompletedMessage(
                       message.ProductId
                       , message.ReservationQuantity
                       , result.Result);
               }, TaskContinuationOptions.AttachedToParent
                   & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });

            Receive<UpdateQuantityMessage>(message =>
           {
               _productInventoryOperations.UpdateQuantity(message.ProductId, message.Quantity).ContinueWith(result =>
                {
                    return new UpdateQuantityCompletedMessage(
                        message.ProductId
                        , message.Quantity
                        , result.Result);
                }, TaskContinuationOptions.AttachedToParent
                       & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });

            Receive<PlaceHoldMessage>(message =>
           {
               _productInventoryOperations.PlaceHold(message.ProductId, message.Holds).ContinueWith(result =>
               {
                   return new PlaceHoldCompletedMessage(
                       message.ProductId
                       , message.Holds
                       , result.Result);
               }, TaskContinuationOptions.AttachedToParent
                  & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });

            Receive<PurchaseMessage>(message =>
           {
               _productInventoryOperations.Purchase(message.ProductId, message.Quantity).ContinueWith(result =>
               {
                   return new PurchaseCompletedMessage(
                       message.ProductId
                       , message.Quantity
                       , result.Result);
               }, TaskContinuationOptions.AttachedToParent
                  & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });

            Receive<PurchaseFromHoldsMessage>(message =>
           {
               _productInventoryOperations.PurchaseFromHolds(message.ProductId, message.Quantity).ContinueWith(result =>
               {
                   return new PurchaseFromHoldsCompletedMessage(
                       message.ProductId
                       , message.Quantity
                       , result.Result);
               }, TaskContinuationOptions.AttachedToParent
                  & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });
            Receive<FlushStreamsMessage>(message =>
            {
                _productInventoryOperations.InventoryStorageFlush(_id).ContinueWith(
                    result => { }, TaskContinuationOptions.AttachedToParent
                  & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
            });
        }
    }
}