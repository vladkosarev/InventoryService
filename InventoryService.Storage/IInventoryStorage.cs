using System;
using System.Threading.Tasks;

namespace InventoryService.Storage
{
    public interface IInventoryStorage : IDisposable
    {
        Task<Tuple<int, int>> ReadInventory(string productId);
        Task<bool> WriteInventory(string productId, int quantity, int reservationQuantity);
        Task Flush(string productId);
    }
}
