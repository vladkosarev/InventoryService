using System;
using System.Globalization;
using System.Numerics;
using System.Threading;

namespace InventoryService.Messages.Models
{
    public class RealTimeInventory : IRealTimeInventory
    {
        public RealTimeInventory(string productId, int quantity, int reserved, int holds)
        {
            ProductId = productId;
            Quantity = quantity;
            Reserved = reserved;
            Holds = holds;
            ETag = GenerateNextGuid();
            UpdatedOn = DateTime.UtcNow;
        }

        public int Quantity { get; }
        public int Reserved { get; }
        public int Holds { get; }
        public string ProductId { get; }
        public Guid ETag { get; }
        public DateTime UpdatedOn { get; }

        public static long GeneratorCounter = 0;

        public static Guid GenerateNextGuid()
        {
            var ticksAsBytes = BitConverter.GetBytes(new DateTime(1900, 1, 1).Ticks);
            Array.Reverse(ticksAsBytes);
            var increment = Interlocked.Increment(ref GeneratorCounter);
            var currentAsBytes = BitConverter.GetBytes(DateTime.UtcNow.AddHours(increment).Ticks);
            Array.Reverse(currentAsBytes);
            var bytes = new byte[16];
            Array.Copy(ticksAsBytes, 0, bytes, 0, ticksAsBytes.Length);
            Array.Copy(currentAsBytes, 0, bytes, 8, currentAsBytes.Length);
            return new Guid(bytes);
        }

        public BigInteger ToBigInteger()
        {
            return BigInteger.Parse(ETag.ToString().Replace("-", ""), NumberStyles.AllowHexSpecifier);
        }
    }
}