using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
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

        public async Task Flush()
        {
        }
    }

    public class FileServiceRepository : IInventoryServiceRepository
    {
        private const string DbFolder = "db";
        private const string FileExtension = ".dbx";

        private class OpenedStream
        {
            public FileStream FileStream { get; private set; }
            public bool Dirty { get; set; }

            public OpenedStream(FileStream fileStream)
            {
                FileStream = fileStream;
            }
        }

        private readonly ConcurrentDictionary<string, OpenedStream> _fileStreams =
            new ConcurrentDictionary<string, OpenedStream>();

        private readonly bool _atomic;
        private Task _flushTask;

        public FileServiceRepository(uint flushInterval = 100, bool atomic = false)
        {
            if (!Directory.Exists(DbFolder)) Directory.CreateDirectory(DbFolder);
            _atomic = atomic;
            //_flushTask = Task.Run(async () =>
            //{
            //    while (!atomic)
            //    {
            //        foreach (var stream in _fileStreams.Values.Where(v => v.Dirty))
            //        {
            //            //await stream.FileStream.FlushAsync();
            //            //stream.FileStream.Flush();
            //            stream.Dirty = false;
            //        }
            //        await Task.Delay(TimeSpan.FromMilliseconds(flushInterval));
            //    }
            //});
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

        //TODO: error checking
        private OpenedStream GetOpenedStream(string fileName, bool createIfDoesNotExist = false)
        {
            OpenedStream openedStream;
            if (_fileStreams.ContainsKey(fileName))
            {
                _fileStreams.TryGetValue(fileName, out openedStream);
            }
            else
            {
                var fileStream = createIfDoesNotExist ?
                    File.Open(fileName, FileMode.OpenOrCreate) : File.Open(fileName, FileMode.Open);
                openedStream = new OpenedStream(fileStream);
                _fileStreams.TryAdd(fileName, openedStream);
            }
            return openedStream;
        }

        private async Task<int> ReadInt(string fileName, int position)
        {
            var result = new byte[sizeof(int)];
            await GetOpenedStream(fileName).FileStream.ReadAsync(result, position, sizeof(int));
            return BitConverter.ToInt32(result, 0);
        }

        private async Task<Tuple<int, int>> ReadConsecutiveInt(string fileName, int position)
        {
            var result = new byte[sizeof(int) * 2];
            var fileStream = GetOpenedStream(fileName).FileStream;
            fileStream.Seek(position, SeekOrigin.Begin);
            await fileStream.ReadAsync(result, 0, sizeof(int) * 2);

            return new Tuple<int, int>(BitConverter.ToInt32(result, 0), BitConverter.ToInt32(result, 4));
        }

        private async Task<bool> WriteInt(string fileName, int position, int value)
        {
            var buffer = BitConverter.GetBytes(value);
            await WriteBuffer(fileName, position, buffer);
            return true;
        }

        private async Task<bool> WriteConsecutiveInt(string fileName, int position, int value1, int value2)
        {
            var buffer = BitConverter.GetBytes(value1).Concat(BitConverter.GetBytes(value2)).ToArray();
            await WriteBuffer(fileName, position, buffer);
            return true;
        }

        private async Task WriteBuffer(string fileName, int position, byte[] buffer)
        {
            var openedStream = GetOpenedStream(fileName, true);
            var fileStream = openedStream.FileStream;
            fileStream.Seek(position, SeekOrigin.Begin);
            await fileStream.WriteAsync(buffer, 0, buffer.Length);
            _fileStreams[fileName].Dirty = true;
            if (_atomic)
            {
                await fileStream.FlushAsync();
                _fileStreams[fileName].Dirty = false;
            }
        }

        public async Task Flush()
        {
            foreach (var stream in _fileStreams.Values.Where(v => v.Dirty))
            {
                await stream.FileStream.FlushAsync();
                stream.Dirty = false;
            }
        }
    }
}