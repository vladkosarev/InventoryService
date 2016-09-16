using InventoryService.Messages;
using InventoryService.Messages.Models;
using InventoryService.Messages.Request;
using InventoryService.Messages.Response;
using InventoryService.Storage;
using System;
using System.Threading.Tasks;

namespace InventoryService.InMemory
{
    public class InventoryServicePureInMemory : IInventoryServiceDirect, IDisposable
    {
        private IInventoryStorage InventoryStorage { set; get; }

        public InventoryServicePureInMemory()
        {
            InventoryStorage = new Storage.InMemoryLib.InMemory();
        }

        public InventoryServicePureInMemory(IInventoryStorage inventoryStorage)
        {
            InventoryStorage = inventoryStorage;
        }

        public async Task<IInventoryServiceCompletedMessage> PurchaseFromHoldsAsync(RealTimeInventory RealTimeInventory, int update)
        {
            var message = new PurchaseFromHoldsMessage(RealTimeInventory.ProductId, update);
            var result =
                await
                    RealTimeInventory.PurchaseFromHoldsAsync(InventoryStorage, message.ProductId, message.Update)
                        .ConfigureAwait(false);
            return result.ProcessAndSendResult(message, (rti) => new PurchaseFromHoldsCompletedMessage(rti, true),
               RealTimeInventory);
        }

        public async Task<IInventoryServiceCompletedMessage> PurchaseAsync(RealTimeInventory RealTimeInventory, int update)
        {
            var message = new PurchaseMessage(RealTimeInventory.ProductId, update);
            var result = await RealTimeInventory.PurchaseAsync(InventoryStorage, message.ProductId, message.Update);
            return result.ProcessAndSendResult(message, (rti) => new PurchaseCompletedMessage(rti, true),
                 RealTimeInventory);
        }

        public async Task<IInventoryServiceCompletedMessage> PlaceHoldAsync(RealTimeInventory RealTimeInventory, int update)
        {
            var message = new PlaceHoldMessage(RealTimeInventory.ProductId, update);
            var result = await RealTimeInventory.PlaceHoldAsync(InventoryStorage, message.ProductId, message.Update);
            return result.ProcessAndSendResult(message, (rti) => new PlaceHoldCompletedMessage(rti, true),
                   RealTimeInventory);
        }

        public async Task<IInventoryServiceCompletedMessage> UpdateQuantityAndHoldAsync(RealTimeInventory RealTimeInventory, int update)
        {
            var message = new UpdateAndHoldQuantityMessage(RealTimeInventory.ProductId, update);
            var updateandHoldResultesult =
                await RealTimeInventory.UpdateQuantityAndHoldAsync(InventoryStorage, message.ProductId, message.Update);
            return updateandHoldResultesult.ProcessAndSendResult(message,
                (rti) => new UpdateAndHoldQuantityCompletedMessage(rti, true), RealTimeInventory);
        }

        public async Task<IInventoryServiceCompletedMessage> UpdateQuantityAsync(RealTimeInventory RealTimeInventory, int update)
        {
            var message = new UpdateQuantityMessage(RealTimeInventory.ProductId, update);
            var result = await RealTimeInventory.UpdateQuantityAsync(InventoryStorage, message.ProductId, message.Update);
            return result.ProcessAndSendResult(message, (rti) => new UpdateQuantityCompletedMessage(rti, true),
                RealTimeInventory);
        }

        public async Task<IInventoryServiceCompletedMessage> ReserveAsync(RealTimeInventory RealTimeInventory, int update)
        {
            var message = new ReserveMessage(RealTimeInventory.ProductId, update);
            var result = await RealTimeInventory.ReserveAsync(InventoryStorage, message.ProductId, message.Update);
            return result.ProcessAndSendResult(message, (rti) => new ReserveCompletedMessage(rti, true),
                   RealTimeInventory);
        }

        public async Task<IInventoryServiceCompletedMessage> GetInventoryAsync(string productId)
        {
            var RealTimeInventory = new RealTimeInventory(productId, 0, 0, 0);
            var message = new GetInventoryMessage(productId);
            var result = await RealTimeInventory.ReadInventoryFromStorageAsync(InventoryStorage, message.ProductId);
            return result.ProcessAndSendResult(message, (rti) => new GetInventoryCompletedMessage(rti, true),
                 RealTimeInventory);
        }

        public void Dispose()
        {
            InventoryStorage.FlushAsync();
            InventoryStorage.Dispose();
        }
    }
}