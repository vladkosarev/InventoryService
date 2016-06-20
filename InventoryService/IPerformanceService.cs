using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace InventoryService
{
    public interface IPerformanceService
    {
        void Init();
        void PrintMetrics();
        void Increment(string counter);
    }

    public class ConsolePerformanceService : IPerformanceService
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly ConcurrentDictionary<string, int> _counters = new ConcurrentDictionary<string, int>();

        public void Init()
        {
            Console.Clear();
            Console.CursorVisible = false;
        }

        public void Increment(string counter)
        {
            _counters.AddOrUpdate(counter, 1, (id, count) => count + 1);
            _counters.AddOrUpdate(counter + "Last", 0, (id, count) => count - 1);
        }

        public void PrintMetrics()
        {
            //_stopwatch.Stop();
            //var reserved = (_reserveMessageCount - _reserveMessageLastCount) / _stopwatch.Elapsed.TotalSeconds;
            //var get = (_getMessageCount - _getMessageLastCount) / _stopwatch.Elapsed.TotalSeconds;
            //var puchase = (_purchaseMessageCount - _purchaseMessageLastCount) / _stopwatch.Elapsed.TotalSeconds;
            //_reserveMessageLastCount = _reserveMessageCount;
            //_getMessageLastCount = _getMessageCount;
            //_purchaseMessageLastCount = _purchaseMessageCount;
            //var width = Console.WindowWidth;
            //Console.ForegroundColor = ConsoleColor.Green;
            //Console.SetCursorPosition(0, 0);
            //Console.Write($"\r\n{(int)get} m/s {_getMessageCount} total reads".PadRight(width));
            //Console.Write($"\r\n{(int)reserved} m/s {_reserveMessageCount} total reservations".PadRight(width));
            //Console.Write($"\r\n{(int)puchase} m/s {_purchaseMessageCount} total purchases".PadRight(width));
            //_stopwatch.Restart();
        }
    }

    public class TestPerformanceService : IPerformanceService
    {
        public void Init()
        {

        }

        public void PrintMetrics()
        {

        }

        public void Increment(string counter)
        {

        }
    }
}