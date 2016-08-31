using Akka.Actor;
using InventoryService.Messages;
using InventoryService.Messages.Models;
using InventoryService.Messages.Request;
using InventoryService.Messages.Response;
using InventoryService.Services;
using InventoryService.Storage;

namespace InventoryService.Actors
{
    public class ProductInventoryActor : ReceiveActor
    {
        private readonly string _id;
        private RealTimeInventory RealTimeInventory { set; get; }
        private readonly IProductInventoryOperations _productInventoryOperations;
        private GetInventoryCompletedMessage GetInventoryCompletedMessageCache { set; get; }
        private readonly bool _withCache;
        public ProductInventoryActor(IInventoryStorage inventoryStorage, string id, bool withCache)
        {
            _id = id;
            _withCache = withCache;

            Become(Running);
            // Become(Experimenting);
            RealTimeInventory=
        }

        private void Running()
        {
            ReceiveAsync<GetInventoryMessage>(async message =>
            {
                if (message.GetNonStaleResult || GetInventoryCompletedMessageCache == null)
                {
                    var result = await _productInventoryOperations.ReadInventory(message.ProductId);

                    if (!result.IsSuccessful)
                    {
                        Sender.Tell(result.ToInventoryOperationErrorMessage(message.ProductId, "Operation failed while trying to get inventory on product " + message.ProductId));
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

                if (!result.IsSuccessful)
                {
                    Sender.Tell(result.ToInventoryOperationErrorMessage(message.ProductId, "Operation failed while trying to do a reserve of " + message.ReservationQuantity + " on product " + message.ProductId));
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
                    if (!result.IsSuccessful)
                    {
                        Sender.Tell(result.ToInventoryOperationErrorMessage(message.ProductId, "Operation failed while trying to do a update quantity of " + message.Quantity + " on product " + message.ProductId));
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

            ReceiveAsync<UpdateAndHoldQuantityMessage>(async message =>
            {
                var updateandHoldResultesult = await _productInventoryOperations.UpdateQuantityAndHold(message.ProductId, message.Quantity);

                {
                    if (updateandHoldResultesult.IsSuccessful)
                    {
                        var quantity = updateandHoldResultesult.Data.Quantity;
                        var reservations = updateandHoldResultesult.Data.Reserved;
                        var holds = updateandHoldResultesult.Data.Holds;
                        GetInventoryCompletedMessageCache = new GetInventoryCompletedMessage(message.ProductId, quantity, reservations, holds);
                        Sender.Tell(new PlaceHoldCompletedMessage(message.ProductId, quantity, reservations, holds, true));
                    }
                    else
                    {
                        Sender.Tell(updateandHoldResultesult.ToInventoryOperationErrorMessage(message.ProductId, "Operation failed while trying to do a update for an update and hold of " + message.Quantity + " on product " + message.ProductId));
                    }
                }
            });

            ReceiveAsync<PlaceHoldMessage>(async message =>
            {
                var result = await _productInventoryOperations.PlaceHold(message.ProductId, message.Holds);

                {
                    if (!result.IsSuccessful)
                    {
                        Sender.Tell(result.ToInventoryOperationErrorMessage(message.ProductId, "Operation failed while trying to do a hold of " + message.Holds + " on product " + message.ProductId));
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

                if (!result.IsSuccessful)
                {
                    Sender.Tell(result.ToInventoryOperationErrorMessage(message.ProductId, "Operation failed while trying to do a purchase of " + message.Quantity + " on product " + message.ProductId));
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
                    if (!result.IsSuccessful)
                    {
                        Sender.Tell(result.ToInventoryOperationErrorMessage(message.ProductId, "Operation failed while trying to do a purchase from hold of " + message.Quantity + " on product " + message.ProductId));
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

        /*
           private void Experimenting()
          {
              Receive<GetInventoryMessage>(message =>
             {
                 if (message.GetNonStaleResult || GetInventoryCompletedMessageCache == null)
                 {
                     _productInventoryOperations.ReadInventory(message.ProductId).ContinueWith(result =>
                         {
                             if (!result.Result.IsSuccessful)
                             {
                                 return
                                     new GetInventoryCompletedMessage(
                                         result.Result.ToInventoryOperationErrorMessage(message.ProductId,
                                             "Operation failed while trying to get inventory on product " +
                                             message.ProductId));
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
                     if (!result.Result.IsSuccessful)
                     {
                         return
                             new ReserveCompletedMessage(
                                 result.Result.ToInventoryOperationErrorMessage(message.ProductId, "Operation failed while trying to do a reserve of " + message.ReservationQuantity + " on product " + message.ProductId));
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
                      if (!result.Result.IsSuccessful)
                      {
                          return
                              new UpdateQuantityCompletedMessage(
                                  result.Result.ToInventoryOperationErrorMessage(message.ProductId, "Operation failed while trying to do a update quantity of " + message.Quantity + " on product " + message.ProductId));
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
                     if (!result.Result.IsSuccessful)
                     {
                         return
                             new PlaceHoldCompletedMessage(
                                 result.Result.ToInventoryOperationErrorMessage(message.ProductId, "Operation failed while trying to do a hold of " + message.Holds + " on product " + message.ProductId));
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
                     if (!result.Result.IsSuccessful)
                     {
                         return
                             new PurchaseCompletedMessage(
                                 result.Result.ToInventoryOperationErrorMessage(message.ProductId, "Operation failed while trying to do a purchase of " + message.Quantity + " on product " + message.ProductId));
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
                     if (!result.Result.IsSuccessful)
                     {
                         return
                             new PurchaseFromHoldsCompletedMessage(
                                 result.Result.ToInventoryOperationErrorMessage(message.ProductId, "Operation failed while trying to do a purchase from hold of " + message.Quantity + " on product " + message.ProductId));
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
           */
    }
}