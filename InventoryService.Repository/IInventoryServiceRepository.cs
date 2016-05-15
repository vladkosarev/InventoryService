using System;
using System.Threading.Tasks;

namespace InventoryService.Repository
{
    public interface IInventoryServiceRepository
    {
        Task<int> ReadQuantity(string productId);
        Task<int> ReadReservations(string productId);
        Task<Tuple<int, int>> ReadQuantityAndReservations(string productId);
        Task<bool> WriteQuantity(string productId, int quantity);
        Task<bool> WriteReservations(string productId, int reservationQuantity);
        Task<bool> WriteQuantityAndReservations(string productId, int quantity, int reservationQuantity);
        Task Flush();
        Task Flush(string productId);
    }
}
