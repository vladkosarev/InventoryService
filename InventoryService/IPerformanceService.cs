
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;

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
        }

        public void PrintMetrics()
        {
            _stopwatch.Stop();
            var width = Console.WindowWidth;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.SetCursorPosition(0, 0);
            foreach (var counter in _counters.Where(k => !k.Key.EndsWith("Last")))
            {
                var lastKey = counter.Key + "Last";
                var lastValue = 0;

                if (_counters.ContainsKey(lastKey))
                {
                    lastValue = _counters[lastKey];
                }

                var value = (counter.Value - lastValue) / _stopwatch.Elapsed.TotalSeconds;
                _counters.AddOrUpdate(lastKey, 0, (id, count) => counter.Value);
                Console.Write($"\r\n{counter.Key} - {(int)value} m/s {counter.Value} total".PadRight(width));

            }
            _stopwatch.Restart();
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
