using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryService.Repository
{
    public class FileServiceRepository : IInventoryServiceRepository, IDisposable
    {
        private readonly string _dbFolder;
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
        private readonly bool _appendMode;

        private Task _flushTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public FileServiceRepository(
            uint flushInterval = 1000
            , string dbFolder = "db"
            , bool atomic = false
            , bool appendMode = true)
        {
            _dbFolder = dbFolder;
            if (!Directory.Exists(_dbFolder)) Directory.CreateDirectory(_dbFolder);
            _atomic = atomic;
            _appendMode = appendMode;

            //_flushTask = Task.Run(async () =>
            //{
            //    while (!atomic && !_cancellationTokenSource.IsCancellationRequested)
            //    {
            //        foreach (var stream in _fileStreams.Values.Where(v => v.Dirty))
            //        {
            //            await stream.FileStream.FlushAsync();
            //            stream.Dirty = false;
            //        }
            //        await Task.Delay(TimeSpan.FromMilliseconds(flushInterval));
            //    }
            //}, _cancellationTokenSource.Token);
        }

        public async Task<int> ReadQuantity(string productId)
        {
            return await ReadInt(Path.Combine(_dbFolder, productId + FileExtension), 0);
        }

        public async Task<int> ReadReservations(string productId)
        {
            return await ReadInt(Path.Combine(_dbFolder, productId + FileExtension), 4);
        }

        public async Task<Tuple<int, int>> ReadQuantityAndReservations(string productId)
        {
            return await ReadConsecutiveInt(Path.Combine(_dbFolder, productId + FileExtension), 0);
        }

        public async Task<bool> WriteQuantity(string productId, int quantity)
        {
            return await WriteInt(Path.Combine(_dbFolder, productId + FileExtension), 0, quantity);
        }

        public async Task<bool> WriteReservations(string productId, int reservationQuantity)
        {
            return await WriteInt(Path.Combine(_dbFolder, productId + FileExtension), 4, reservationQuantity);
        }

        public async Task<bool> WriteQuantityAndReservations(string productId, int quantity, int reservationQuantity)
        {
            return
                await
                    WriteConsecutiveInt(Path.Combine(_dbFolder, productId + FileExtension), 0, quantity,
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
                var mode = createIfDoesNotExist ? FileMode.OpenOrCreate : FileMode.Open;

                var fileStream = new FileStream(fileName,
                    mode, FileAccess.ReadWrite, FileShare.Read,
                    bufferSize: 4096, useAsync: true);

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
            if (_appendMode)
                fileStream.Seek(-sizeof(int) * 2 + position, SeekOrigin.End);
            else
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
            if (!_appendMode)
                fileStream.Seek(fileStream.Length - position, SeekOrigin.End);
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

        public async Task Flush(string productId)
        {
            await _fileStreams[Path.Combine(_dbFolder, productId + FileExtension)].FileStream.FlushAsync();
            _fileStreams[Path.Combine(_dbFolder, productId + FileExtension)].Dirty = false;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            foreach (var stream in _fileStreams.Values)
            {
                stream.FileStream.Close();
                stream.Dirty = false;
            }
        }
    }
}