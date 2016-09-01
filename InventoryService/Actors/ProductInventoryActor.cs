﻿using System;
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
        private readonly bool _withCache;
        private IInventoryStorage InventoryStorage { set; get; }

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
                if (message.GetNonStaleResult)
                {
                    var result = await RealTimeInventory.ReadInventoryFromStorageAsync(InventoryStorage, message.ProductId);
                    ProcessAndSendResult(result, message, (rti) => new GetInventoryCompletedMessage(rti, true));
                }
            });

            ReceiveAsync<ReserveMessage>(async message =>
            {
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
                ProcessAndSendResult(result, message,(rti)=> new PurchaseFromHoldsCompletedMessage(rti, true));
            });

            ReceiveAsync<FlushStreamsMessage>(async message =>
            {
                var result = await RealTimeInventory.InventoryStorageFlushAsync(InventoryStorage, _id);
                Sender.Tell(result.Data);
            });
        }

        private void ProcessAndSendResult(OperationResult<IRealTimeInventory> result, IRequestMessage requestMessage, Func<RealTimeInventory, IInventoryServiceCompletedMessage> successResponseCompletedMessage)
        {
            if (!result.IsSuccessful)
            {
           

                Sender.Tell(result.ToInventoryOperationErrorMessage(
                    requestMessage.ProductId
                    ,"Operation failed while trying to " + requestMessage.GetType() + " with update " + requestMessage.Update + " on product " + requestMessage.ProductId));
            }
            else
            {
                RealTimeInventory = result.Data as RealTimeInventory;
                Sender.Tell(successResponseCompletedMessage(RealTimeInventory));
            }
        }
    }
}