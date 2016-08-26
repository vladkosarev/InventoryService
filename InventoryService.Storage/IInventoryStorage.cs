using System;
using System.Threading.Tasks;

namespace InventoryService.Storage
{
    public interface IInventoryStorage : IDisposable
    {
        Task<RealTimeInventory> ReadInventory(string productId);

        Task<bool> WriteInventory(RealTimeInventory inventoryObject);

        Task<bool> Flush(string productId);
    }
}