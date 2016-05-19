using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Raven.Client;
using Raven.Client.Document;

namespace InventoryService.Storage
{
    public class RavenDB : IInventoryStorage, IDisposable
    {
        [Serializable]
        class RealTimeInventory
        {
            public int Quantity;
            public int Reservations;

            public RealTimeInventory(int quantity, int reservations)
            {
                Quantity = quantity;
                Reservations = reservations;
            }

        }

        private IDocumentStore _documentStore;

        public RavenDB()
        {
            _documentStore = new DocumentStore
            {
                ConnectionStringName = "RavenDB"
            };
            _documentStore.Initialize();
        }

        private string ConvertId(string productId)
        {
            var number = Regex.Match(productId, @"\d+").Value;
            return "RealTimeInventory/" + number;
        }

        public async Task<Tuple<int, int>> ReadInventory(string productId)
        {
            RealTimeInventory inventory;
            using (var session = _documentStore.OpenAsyncSession())
            {
                inventory = await session.LoadAsync<RealTimeInventory>(ConvertId(productId));
            }
            return new Tuple<int, int>(inventory.Quantity, inventory.Reservations);
        }

        public async Task<bool> WriteInventory(string productId, int quantity, int reservationQuantity)
        {
            var inventory = new RealTimeInventory(quantity, reservationQuantity);
            using (var session = _documentStore.OpenAsyncSession())
            {
                await session.StoreAsync(inventory, ConvertId(productId));
                await session.SaveChangesAsync();
            }
            return true;
        }

        public async Task Flush(string productId)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _documentStore.Dispose();
        }
    }
}
