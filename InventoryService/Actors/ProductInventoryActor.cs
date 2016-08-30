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

        private readonly IProductInventoryOperations _productInventoryOperations;
        private GetInventoryCompletedMessage GetInventoryCompletedMessageCache { set; get; }

        public ProductInventoryActor(IInventoryStorage inventoryStorage, string id)
        {
            _id = id;
            Become(Running);
            // Become(Experimenting);
            _productInventoryOperations = new ProductInventoryOperations(inventoryStorage, id);
        }

        private void Running()
        {
            ReceiveAsync<GetInventoryMessage>(async message =>
            {
                if (message.GetNonStaleResult || GetInventoryCompletedMessageCache == null)
                {
                    var result = await _productInventoryOperations.ReadInventory(message.ProductId);

                    var thatMessage = message;
                    if (!result.IsSuccessful)
                    {
                        Sender.Tell(new GetInventoryCompletedMessage(
                                   result.ToInventoryOperationErrorMessage(thatMessage.ProductId, "Operation failed while trying to get inventory on product " + thatMessage.ProductId)));
                    }
                    else
                    {
                        var quantity = result.Data.Quantity;
                        var reservations = result.Data.Reserved;
                        var holds = result.Data.Holds;

                        GetInventoryCompletedMessageCache = new GetInventoryCompletedMessage(message.ProductId, quantity, reservations, holds);
                    }
                }
                Sender.Tell(GetInventoryCompletedMessageCache);
            });

            ReceiveAsync<ReserveMessage>(async message =>
            {
                var result = await _productInventoryOperations.Reserve(message.ProductId, message.ReservationQuantity);

                var thatMessage = message;
                if (!result.IsSuccessful)
                {
                    Sender.Tell(new ReserveCompletedMessage(
                            result.ToInventoryOperationErrorMessage(thatMessage.ProductId, "Operation failed while trying to do a reserve of " + thatMessage.ReservationQuantity + " on product " + thatMessage.ProductId)));
                }
                else
                {
                    var quantity = result.Data.Quantity;
                    var reservations = result.Data.Reserved;
                    var holds = result.Data.Holds;
                    GetInventoryCompletedMessageCache = new GetInventoryCompletedMessage(message.ProductId, quantity, reservations, holds);
                    Sender.Tell(new ReserveCompletedMessage(message.ProductId, quantity, reservations, holds, true));
                }
            });

            ReceiveAsync<UpdateQuantityMessage>(async message =>
            {
                var result = await _productInventoryOperations.UpdateQuantity(message.ProductId, message.Quantity);

                {
                    var thatMessage = message;
                    if (!result.IsSuccessful)
                    {
                        Sender.Tell(new UpdateQuantityCompletedMessage(
                                   result.ToInventoryOperationErrorMessage(thatMessage.ProductId, "Operation failed while trying to do a update quantity of " + thatMessage.Quantity + " on product " + thatMessage.ProductId)));
                    }
                    else
                    {
                        var quantity = result.Data.Quantity;
                        var reservations = result.Data.Reserved;
                        var holds = result.Data.Holds;
                        GetInventoryCompletedMessageCache = new GetInventoryCompletedMessage(message.ProductId, quantity, reservations, holds);
                        Sender.Tell(new UpdateQuantityCompletedMessage(message.ProductId, quantity, reservations, holds, true));
                    }
                }
            });

            ReceiveAsync<PlaceHoldMessage>(async message =>
            {
                var result = await _productInventoryOperations.PlaceHold(message.ProductId, message.Holds);

                {
                    var thatMessage = message;
                    if (!result.IsSuccessful)
                    {
                        Sender.Tell(new PlaceHoldCompletedMessage(
                                result.ToInventoryOperationErrorMessage(thatMessage.ProductId, "Operation failed while trying to do a hold of " + thatMessage.Holds + " on product " + thatMessage.ProductId)));
                    }
                    else
                    {
                        var quantity = result.Data.Quantity;
                        var reservations = result.Data.Reserved;
                        var holds = result.Data.Holds;
                        GetInventoryCompletedMessageCache = new GetInventoryCompletedMessage(message.ProductId, quantity, reservations, holds);
                        Sender.Tell(new PlaceHoldCompletedMessage(message.ProductId, quantity, reservations, holds, true));
                    }
                }
            });

            ReceiveAsync<PurchaseMessage>(async message =>
            {
                var result = await _productInventoryOperations.Purchase(message.ProductId, message.Quantity);

                var thatMessage = message;
                if (!result.IsSuccessful)
                {
                    Sender.Tell(new PurchaseCompletedMessage(
                              result.ToInventoryOperationErrorMessage(thatMessage.ProductId, "Operation failed while trying to do a purchase of " + thatMessage.Quantity + " on product " + thatMessage.ProductId)));
                }
                else
                {
                    var quantity = result.Data.Quantity;
                    var reservations = result.Data.Reserved;
                    var holds = result.Data.Holds;
                    GetInventoryCompletedMessageCache = new GetInventoryCompletedMessage(message.ProductId, quantity, reservations, holds);
                    Sender.Tell(new PurchaseCompletedMessage(message.ProductId, quantity, reservations, holds, true));
                }
            });

            ReceiveAsync<PurchaseFromHoldsMessage>(async message =>
            {
                var result = await _productInventoryOperations.PurchaseFromHolds(message.ProductId, message.Quantity).ConfigureAwait(false);

                {
                    var thatMessage = message;
                    if (!result.IsSuccessful)
                    {
                        Sender.Tell(new PurchaseFromHoldsCompletedMessage(
                                 result.ToInventoryOperationErrorMessage(thatMessage.ProductId, "Operation failed while trying to do a purchase from hold of " + thatMessage.Quantity + " on product " + thatMessage.ProductId)));
                    }
                    else
                    {
                        var quantity = result.Data.Quantity;
                        var reservations = result.Data.Reserved;
                        var holds = result.Data.Holds;
                        GetInventoryCompletedMessageCache = new GetInventoryCompletedMessage(message.ProductId, quantity, reservations, holds);
                        Sender.Tell(new PurchaseFromHoldsCompletedMessage(message.ProductId, quantity, reservations, holds, true));
                    }
                }
            });

            ReceiveAsync<FlushStreamsMessage>(async message =>
            {
                var result = await _productInventoryOperations.InventoryStorageFlush(_id);
                Sender.Tell(result.Data);
            });
        }

        private void Experimenting()
        {
            Receive<GetInventoryMessage>(message =>
           {
               if (message.GetNonStaleResult || GetInventoryCompletedMessageCache == null)
               {
                   _productInventoryOperations.ReadInventory(message.ProductId).ContinueWith(result =>
                       {
                           var thatMessage = message;
                           if (!result.Result.IsSuccessful)
                           {
                               return
                                   new GetInventoryCompletedMessage(
                                       result.Result.ToInventoryOperationErrorMessage(thatMessage.ProductId,
                                           "Operation failed while trying to get inventory on product " +
                                           thatMessage.ProductId));
                           }

                           var quantity = result.Result.Data.Quantity;
                           var reservations = result.Result.Data.Reserved;
                           var holds = result.Result.Data.Holds;
                           return new GetInventoryCompletedMessage(message.ProductId, quantity, reservations, holds);
                       },
                       TaskContinuationOptions.AttachedToParent
                       & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
               }
               else
               {
                   Sender.Tell(GetInventoryCompletedMessageCache);
               }
           });

            Receive<ReserveMessage>(message =>
           {
               _productInventoryOperations.Reserve(message.ProductId, message.ReservationQuantity).ContinueWith(result =>
               {
                   var thatMessage = message;
                   if (!result.Result.IsSuccessful)
                   {
                       return
                           new ReserveCompletedMessage(
                               result.Result.ToInventoryOperationErrorMessage(thatMessage.ProductId, "Operation failed while trying to do a reserve of " + thatMessage.ReservationQuantity + " on product " + thatMessage.ProductId));
                   }

                   var quantity = result.Result.Data.Quantity;
                   var reservations = result.Result.Data.Reserved;
                   var holds = result.Result.Data.Holds;
                   return new ReserveCompletedMessage(message.ProductId, quantity, reservations, holds, true);
               }, TaskContinuationOptions.AttachedToParent
                   & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });

            Receive<UpdateQuantityMessage>(message =>
           {
               _productInventoryOperations.UpdateQuantity(message.ProductId, message.Quantity).ContinueWith(result =>
                {
                    var thatMessage = message;
                    if (!result.Result.IsSuccessful)
                    {
                        return
                            new UpdateQuantityCompletedMessage(
                                result.Result.ToInventoryOperationErrorMessage(thatMessage.ProductId, "Operation failed while trying to do a update quantity of " + thatMessage.Quantity + " on product " + thatMessage.ProductId));
                    }

                    var quantity = result.Result.Data.Quantity;
                    var reservations = result.Result.Data.Reserved;
                    var holds = result.Result.Data.Holds;
                    return new UpdateQuantityCompletedMessage(message.ProductId, quantity, reservations, holds, true);
                }, TaskContinuationOptions.AttachedToParent
                       & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });

            Receive<PlaceHoldMessage>(message =>
           {
               _productInventoryOperations.PlaceHold(message.ProductId, message.Holds).ContinueWith(result =>
               {
                   var thatMessage = message;
                   if (!result.Result.IsSuccessful)
                   {
                       return
                           new PlaceHoldCompletedMessage(
                               result.Result.ToInventoryOperationErrorMessage(thatMessage.ProductId, "Operation failed while trying to do a hold of " + thatMessage.Holds + " on product " + thatMessage.ProductId));
                   }

                   var quantity = result.Result.Data.Quantity;
                   var reservations = result.Result.Data.Reserved;
                   var holds = result.Result.Data.Holds;
                   return new PlaceHoldCompletedMessage(message.ProductId, quantity, reservations, holds, true);
               }, TaskContinuationOptions.AttachedToParent
                  & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });

            Receive<PurchaseMessage>(message =>
           {
               _productInventoryOperations.Purchase(message.ProductId, message.Quantity).ContinueWith(result =>
               {
                   var thatMessage = message;
                   if (!result.Result.IsSuccessful)
                   {
                       return
                           new PurchaseCompletedMessage(
                               result.Result.ToInventoryOperationErrorMessage(thatMessage.ProductId, "Operation failed while trying to do a purchase of " + thatMessage.Quantity + " on product " + thatMessage.ProductId));
                   }

                   var quantity = result.Result.Data.Quantity;
                   var reservations = result.Result.Data.Reserved;
                   var holds = result.Result.Data.Holds;
                   return new PurchaseCompletedMessage(message.ProductId, quantity, reservations, holds, true);
               }, TaskContinuationOptions.AttachedToParent
                  & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });

            Receive<PurchaseFromHoldsMessage>(message =>
           {
               _productInventoryOperations.PurchaseFromHolds(message.ProductId, message.Quantity).ContinueWith(result =>
               {
                   var thatMessage = message;
                   if (!result.Result.IsSuccessful)
                   {
                       return
                           new PurchaseFromHoldsCompletedMessage(
                               result.Result.ToInventoryOperationErrorMessage(thatMessage.ProductId, "Operation failed while trying to do a purchase from hold of " + thatMessage.Quantity + " on product " + thatMessage.ProductId));
                   }

                   var quantity = result.Result.Data.Quantity;
                   var reservations = result.Result.Data.Reserved;
                   var holds = result.Result.Data.Holds;
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