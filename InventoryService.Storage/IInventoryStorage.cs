using System;
using System.Threading.Tasks;

namespace InventoryService.Storage
{
    public interface IInventoryStorage : IDisposable
    {
        // Quantity, Reservations, Holds
        Task<Tuple<int, int, int>> ReadInventory(string productId);
        Task<bool> WriteInventory(string productId, int quantity, int reservations, int holds);
        Task Flush(string productId);
    }
}
