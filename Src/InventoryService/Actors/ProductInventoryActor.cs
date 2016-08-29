using Akka.Actor;
using InventoryService.Messages;
using InventoryService.Messages.Request;
using InventoryService.Messages.Response;
using InventoryService.Services;
using InventoryService.Storage;
using System.Threading.Tasks;

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
            _productInventoryOperations = new ProductInventoryOperations(inventoryStorage, id);
        }

        private void Running()
        {
            Receive<GetInventoryMessage>(message =>
           {
               if (!_withCache)
               {
                   _productInventoryOperations.ReadInventory(message.ProductId).ContinueWith(result =>
                   {
                       if (!result.Result.IsSuccessful)
                       {
                           return
                               new GetInventoryCompletedMessage(
                                   result.Result.Exception.ToInventoryOperationErrorMessage(message.ProductId));
                       }
                       var quantity = result.Result.Result.Quantity;
                       var reservations = result.Result.Result.Reservations;
                       var holds = result.Result.Result.Holds;
                       return new GetInventoryCompletedMessage(message.ProductId, quantity, reservations, holds);
                   },
                   TaskContinuationOptions.AttachedToParent
                   & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
               }
           });

            Receive<ReserveMessage>(message =>
           {
               _productInventoryOperations.Reserve(message.ProductId, message.ReservationQuantity).ContinueWith(result =>
               {
                   if (!result.Result.IsSuccessful)
                   {
                       return
                           new ReserveCompletedMessage(
                               result.Result.Exception.ToInventoryOperationErrorMessage(message.ProductId));
                   }

                   var quantity = result.Result.Result.Quantity;
                   var reservations = result.Result.Result.Reservations;
                   var holds = result.Result.Result.Holds;
                   return new ReserveCompletedMessage(message.ProductId, quantity, reservations, holds, true);
               }, TaskContinuationOptions.AttachedToParent
                   & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });

            Receive<UpdateQuantityMessage>(message =>
           {
               _productInventoryOperations.UpdateQuantity(message.ProductId, message.Quantity).ContinueWith(result =>
                {
                    if (!result.Result.IsSuccessful)
                    {
                        return
                            new UpdateQuantityCompletedMessage(
                                result.Result.Exception.ToInventoryOperationErrorMessage(message.ProductId));
                    }

                    var quantity = result.Result.Result.Quantity;
                    var reservations = result.Result.Result.Reservations;
                    var holds = result.Result.Result.Holds;
                    return new UpdateQuantityCompletedMessage(message.ProductId, quantity, reservations, holds, true);
                }, TaskContinuationOptions.AttachedToParent
                       & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });

            Receive<PlaceHoldMessage>(message =>
           {
               _productInventoryOperations.PlaceHold(message.ProductId, message.Holds).ContinueWith(result =>
               {
                   if (!result.Result.IsSuccessful)
                   {
                       return
                           new PlaceHoldCompletedMessage(
                               result.Result.Exception.ToInventoryOperationErrorMessage(message.ProductId));
                   }

                   var quantity = result.Result.Result.Quantity;
                   var reservations = result.Result.Result.Reservations;
                   var holds = result.Result.Result.Holds;
                   return new PlaceHoldCompletedMessage(message.ProductId, quantity, reservations, holds, true);
               }, TaskContinuationOptions.AttachedToParent
                  & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });

            Receive<PurchaseMessage>(message =>
           {
               _productInventoryOperations.Purchase(message.ProductId, message.Quantity).ContinueWith(result =>
               {
                   if (!result.Result.IsSuccessful)
                   {
                       return
                           new PurchaseCompletedMessage(
                               result.Result.Exception.ToInventoryOperationErrorMessage(message.ProductId));
                   }

                   var quantity = result.Result.Result.Quantity;
                   var reservations = result.Result.Result.Reservations;
                   var holds = result.Result.Result.Holds;
                   return new PurchaseCompletedMessage(message.ProductId, quantity, reservations, holds, true);
               }, TaskContinuationOptions.AttachedToParent
                  & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });

            Receive<PurchaseFromHoldsMessage>(message =>
           {
               _productInventoryOperations.PurchaseFromHolds(message.ProductId, message.Quantity).ContinueWith(result =>
               {
                   if (!result.Result.IsSuccessful)
                   {
                       return
                           new PurchaseFromHoldsCompletedMessage(
                               result.Result.Exception.ToInventoryOperationErrorMessage(message.ProductId));
                   }

                   var quantity = result.Result.Result.Quantity;
                   var reservations = result.Result.Result.Reservations;
                   var holds = result.Result.Result.Holds;
                   return new PurchaseFromHoldsCompletedMessage(message.ProductId, quantity, reservations, holds, true);
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