using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InventoryService.Repository;

namespace InventoryService.Storage
{
    public class FileSystem : IInventoryServiceRepository, IDisposable
    {
        private readonly string _dbFolder;
        private readonly string _dbFile;
        private readonly string _fileExtension = ".dbx";
        private static SemaphoreSlim semaphore = new SemaphoreSlim(1,1);

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

        private OpenedStream _openedStream;
        private readonly bool _atomic;

        private Task _flushTask;

        //todo deal with this
        public Dictionary<string, int> Cache { get; private set; }

        public FileSystem(
            uint flushInterval = 100
            , string dbFolder = "db"
            , string dbFile = "data"
            , bool atomic = false)
        {
            _dbFolder = dbFolder;
            if (!Directory.Exists(_dbFolder)) Directory.CreateDirectory(_dbFolder);
            _atomic = atomic;
            _dbFile = dbFile;
            var fullPath = Path.Combine(_dbFolder, _dbFile + _fileExtension);

            using (var stream = new FileStream(fullPath,
                FileMode.OpenOrCreate, FileAccess.Read, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                _openedStream = new OpenedStream(stream);
                Cache = GetAll().Result;
            }

            _openedStream = new OpenedStream(new FileStream(fullPath,
                FileMode.Append, FileAccess.Write, FileShare.Read,
                bufferSize: 4096, useAsync: true));
        }

        public async Task<int> ReadQuantity(string productId)
        {
            return await ReadInt(Path.Combine(_dbFolder, productId + _fileExtension), 0);
        }

        public async Task<int> ReadReservations(string productId)
        {
            return await ReadInt(Path.Combine(_dbFolder, productId + _fileExtension), 4);
        }

        public async Task<Tuple<int, int>> ReadQuantityAndReservations(string productId)
        {
            return await ReadConsecutiveInt(Path.Combine(_dbFolder, productId + _fileExtension), 0);
        }

        public async Task<bool> WriteQuantity(string productId, int quantity)
        {
            return await WriteInt(Path.Combine(_dbFolder, productId + _fileExtension), 0, quantity);
        }

        public async Task<bool> WriteReservations(string productId, int reservationQuantity)
        {
            return await WriteInt(Path.Combine(_dbFolder, productId + _fileExtension), 4, reservationQuantity);
        }

        public async Task<bool> WriteQuantityAndReservations(string productId, int quantity, int reservationQuantity)
        {
            return
                await WriteConsecutiveInt(
                        productId + "/q"
                        , quantity
                        , productId + "/r"
                        , reservationQuantity);
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
                fileStream.Seek(0, SeekOrigin.End);
                openedStream = new OpenedStream(fileStream);
                _fileStreams.TryAdd(fileName, openedStream);
            }
            return openedStream;
        }

        private async Task<int> ReadInt(string fileName, int position)
        {
            var result = new byte[sizeof(int)];
            await GetOpenedStream(fileName).FileStream.ReadAsync(result, 0, sizeof(int));
            return BitConverter.ToInt32(result, 0);
        }

        private async Task<Tuple<int, int>> ReadConsecutiveInt(string fileName, int position)
        {
            var result = new byte[sizeof(int) * 2];
            var fileStream = GetOpenedStream(fileName).FileStream;

            await fileStream.ReadAsync(result, 0, sizeof(int) * 2);

            return new Tuple<int, int>(BitConverter.ToInt32(result, 0), BitConverter.ToInt32(result, 4));
        }

        private async Task<bool> WriteInt(string fileName, int position, int value)
        {
            var buffer = BitConverter.GetBytes(value);
            await WriteBuffer(buffer);
            return true;
        }

        private IEnumerable<byte> ToByteArray(string name, int value)
        {
            var n = Encoding.ASCII.GetBytes(name.PadRight(16));
            var v = BitConverter.GetBytes(value);
            return n.Concat(v);
        }

        private async Task<bool> WriteConsecutiveInt(string id1, int value1, string id2, int value2)
        {
            var buffer =
                ToByteArray(id1, value1)
                .Concat(ToByteArray(id2, value2))
                .ToArray();

            await WriteBuffer(buffer);
            return true;
        }

        private async Task WriteBuffer(byte[] buffer)
        {
            var openedStream = _openedStream;
            var fileStream = openedStream.FileStream;

            await semaphore.WaitAsync();
            try
            {
                await fileStream.WriteAsync(buffer, 0, buffer.Length);
                openedStream.Dirty = true;
                if (_atomic)
                {
                    await fileStream.FlushAsync();
                    openedStream.Dirty = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: {0}", ex.Message);
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task Flush()
        {
            await _openedStream.FileStream.FlushAsync();
        }

        public async Task<Dictionary<string, int>> GetAll()
        {
            //todo really bad code
            if (Cache != null) return Cache;

            var stream = _openedStream.FileStream;
            var buffer = new byte[stream.Length];
            var x = await stream.ReadAsync(
                buffer
                , 0
                , buffer.Length);

            // TODO parallelize, etc
            var pairs = new Dictionary<string, int>();
            for (var i = 0; i < buffer.Length / 20; i++)
            {
                var key = Encoding.ASCII.GetString(buffer, 20 * i, 16).TrimEnd();
                var value = BitConverter.ToInt32(buffer, 20 * i + 16);
                if (pairs.ContainsKey(key)) pairs[key] = value;
                else pairs.Add(key, value);
            }

            return pairs;
        }

        public void Dispose()
        {
            _openedStream.FileStream.Close();            
        }
    }
}

