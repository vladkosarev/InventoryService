using Akka.Actor;
using InventoryService.Messages;
using InventoryService.Storage;
using System;
using System.Threading.Tasks;

namespace InventoryService.Actors
{
    public class ProductInventoryActor : ReceiveActor
    {
        private readonly string _id;
        private int _quantity;
        private int _reservations;
        private int _holds;

        private readonly bool _withCache;

        private readonly IInventoryStorage _inventoryStorage;

        public ProductInventoryActor(IInventoryStorage inventoryStorage, string id, bool withCache)
        {
            _id = id;
            _inventoryStorage = inventoryStorage;
            _withCache = withCache;
            var initTask = _inventoryStorage.ReadInventory(id);
            Task.WaitAll(initTask);
            var inventory = initTask.Result;
            _quantity = inventory.Quantity;
            _reservations = inventory.Reservations;
            _holds = inventory.Holds;

            Become(Running);

            //Context.System.Scheduler.ScheduleTellRepeatedly(
            //    TimeSpan.Zero
            //    , TimeSpan.FromMilliseconds(100)
            //    , Self
            //    , new FlushStreamMessage(_id)
            //    , ActorRefs.Nobody);
        }

        private async Task<bool> Reserve(string productId, int reservationQuantity)
        {
            var newReserved = _reservations + reservationQuantity;
            if (newReserved > _quantity - _holds) return false;
            var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, _quantity, newReserved, _holds));

            if (!result) return false;
            _reservations = newReserved;
            return true;
        }

        private async Task<bool> UpdateQuantity(string productId, int quantity)
        {
            var newQuantity = _quantity + quantity;

            var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, _reservations, _holds));

            if (!result) return false;
            _quantity = newQuantity;
            return true;
        }

        private async Task<bool> PlaceHold(string productId, int toHold)
        {
            var newHolds = _holds + toHold;
            if (newHolds > _quantity) return false;
            var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, _quantity, _reservations, newHolds));

            if (!result) return false;
            _holds = newHolds;
            return true;
        }

        private async Task<bool> Purchase(string productId, int quantity)
        {
            var newQuantity = _quantity - quantity;
            var newReserved = Math.Max(0, _reservations - quantity);

            if (newQuantity - _holds < 0) return false;
            var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, newReserved, _holds));

            if (!result) return false;
            _quantity = newQuantity;
            _reservations = newReserved;
            return true;
        }

        private async Task<bool> PurchaseFromHolds(string productId, int quantity)
        {
            var newQuantity = _quantity - quantity;
            var newHolds = _holds - quantity;

            if (newQuantity < 0 || newHolds < 0) return false;
            var result = await _inventoryStorage.WriteInventory(new RealTimeInventory(productId, newQuantity, _reservations, newHolds));

            if (!result) return false;
            _quantity = newQuantity;
            _holds = newHolds;
            return true;
        }

        private void Running()
        {
            Receive<GetInventoryMessage>(message =>
           {
               if (!_withCache)
               {
                   _inventoryStorage.ReadInventory(message.ProductId).ContinueWith(result =>
                   {
                       _quantity = result.Result.Quantity;
                       _reservations = result.Result.Reservations;
                       _holds = result.Result.Holds;
                       return new RetrieveInventoryCompletedMessage(message.ProductId, _quantity, _reservations);
                   },
                   TaskContinuationOptions.AttachedToParent
                   & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
               }
           });

            Receive<ReserveMessage>(message =>
           {
               Reserve(message.ProductId, message.ReservationQuantity).ContinueWith(result =>
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
               UpdateQuantity(message.ProductId, message.Quantity).ContinueWith(result =>
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
               PlaceHold(message.ProductId, message.Holds).ContinueWith(result =>
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
               Purchase(message.ProductId, message.Quantity).ContinueWith(result =>
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
               PurchaseFromHolds(message.ProductId, message.Quantity).ContinueWith(result =>
               {
                   return new PurchaseFromHoldsCompletedMessage(
                       message.ProductId
                       , message.Quantity
                       , result.Result);
               }, TaskContinuationOptions.AttachedToParent
                  & TaskContinuationOptions.ExecuteSynchronously).PipeTo(Sender);
           });
            Receive<FlushStreamsMessage>(message => { _inventoryStorage.Flush(_id); });
        }
    }
}