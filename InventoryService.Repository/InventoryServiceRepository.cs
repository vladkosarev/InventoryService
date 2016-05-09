using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
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
    }

    public class InMemoryInventoryServiceRepository : IInventoryServiceRepository
    {
        private readonly ConcurrentDictionary<string, Tuple<int, int>> _productInventories =
            new ConcurrentDictionary<string, Tuple<int, int>>();

        public async Task<int> ReadQuantity(string productId)
        {
            if (!_productInventories.ContainsKey(productId))
                throw new InvalidOperationException();
            return _productInventories[productId].Item1;
        }

        public async Task<int> ReadReservations(string productId)
        {
            if (!_productInventories.ContainsKey(productId))
                throw new InvalidOperationException();
            return _productInventories[productId].Item2;
        }

        public async Task<Tuple<int, int>> ReadQuantityAndReservations(string productId)
        {
            if (!_productInventories.ContainsKey(productId))
                throw new InvalidOperationException();
            return _productInventories[productId];
        }

        public async Task<bool> WriteQuantity(string productId, int quantity)
        {
            if (!_productInventories.ContainsKey(productId))
                _productInventories.TryAdd(productId, new Tuple<int, int>(quantity, 0));
            else
                _productInventories[productId] = new Tuple<int, int>(quantity, _productInventories[productId].Item2);
            return true;
        }

        public async Task<bool> WriteReservations(string productId, int reservationQuantity)
        {
            if (!_productInventories.ContainsKey(productId))
                _productInventories.TryAdd(productId, new Tuple<int, int>(0, reservationQuantity));
            else
                _productInventories[productId] = new Tuple<int, int>(_productInventories[productId].Item1,
                    reservationQuantity);
            return true;
        }

        public async Task<bool> WriteQuantityAndReservations(string productId, int quantity, int reservationQuantity)
        {
            if (!_productInventories.ContainsKey(productId))
                _productInventories.TryAdd(productId, new Tuple<int, int>(quantity, reservationQuantity));
            else
                _productInventories[productId] = new Tuple<int, int>(quantity, reservationQuantity);
            return true;
        }
    }

    public class FileServiceRepository : IInventoryServiceRepository
    {
        private const string DbFolder = "db";
        private const string FileExtension = ".dbx";

        public FileServiceRepository()
        {
            if (!Directory.Exists(DbFolder)) Directory.CreateDirectory(DbFolder);
        }

        public async Task<int> ReadQuantity(string productId)
        {
            return await ReadInt(Path.Combine(DbFolder, productId + FileExtension), 0);
        }

        public async Task<int> ReadReservations(string productId)
        {
            return await ReadInt(Path.Combine(DbFolder, productId + FileExtension), 4);
        }

        public async Task<Tuple<int, int>> ReadQuantityAndReservations(string productId)
        {
            return await ReadConsecutiveInt(Path.Combine(DbFolder, productId + FileExtension), 0);
        }

        public async Task<bool> WriteQuantity(string productId, int quantity)
        {
            return await WriteInt(Path.Combine(DbFolder, productId + FileExtension), 0, quantity);
        }

        public async Task<bool> WriteReservations(string productId, int reservationQuantity)
        {
            return await WriteInt(Path.Combine(DbFolder, productId + FileExtension), 4, reservationQuantity);
        }

        public async Task<bool> WriteQuantityAndReservations(string productId, int quantity, int reservationQuantity)
        {
            return
                await
                    WriteConsecutiveInt(Path.Combine(DbFolder, productId + FileExtension), 0, quantity,
                        reservationQuantity);
        }

        private async Task<int> ReadInt(string file, int position)
        {
            var filename = file;
            byte[] result;

            using (var stream = File.Open(filename, FileMode.Open))
            {
                result = new byte[sizeof(int)];
                await stream.ReadAsync(result, position, sizeof(int));
            }

            return BitConverter.ToInt32(result, 0);
        }

        private async Task<bool> WriteInt(string file, int position, int value)
        {
            var filename = file;
            var buffer = BitConverter.GetBytes(value);

            using (var stream = File.Open(filename, FileMode.OpenOrCreate))
            {
                await stream.WriteAsync(buffer, position, buffer.Length);
            }

            return true;
        }

        private static async Task<Tuple<int, int>> ReadConsecutiveInt(string file, int position)
        {
            var filename = file;
            byte[] result;

            using (var stream = File.Open(filename, FileMode.Open))
            {
                result = new byte[sizeof(int)*2];
                await stream.ReadAsync(result, position, sizeof(int)*2);
            }

            return new Tuple<int, int>(BitConverter.ToInt32(result, 0), BitConverter.ToInt32(result, 4));
        }

        private async Task<bool> WriteConsecutiveInt(string file, int position, int value1, int value2)
        {
            var filename = file;
            var buffer = BitConverter.GetBytes(value1).Concat(BitConverter.GetBytes(value2)).ToArray();

            using (var stream = File.Open(filename, FileMode.OpenOrCreate))
            {
                await stream.WriteAsync(buffer, position, buffer.Length);
            }

            return true;
        }
    }
}