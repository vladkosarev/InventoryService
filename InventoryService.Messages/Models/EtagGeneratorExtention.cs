using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading;

namespace InventoryService.Messages.Models
{
    public static class EtagGeneratorExtention
    {

        public static long GlobalEtagGeneratorCounter = 0;

        public static Guid GenerateNextGuid(this IRealTimeInventory inventory)
        {
            var ticksAsBytes = BitConverter.GetBytes(new DateTime(1900, 1, 1).Ticks);
            Array.Reverse(ticksAsBytes);
            var increment = Interlocked.Increment(ref GlobalEtagGeneratorCounter);
            var currentAsBytes = BitConverter.GetBytes(DateTime.UtcNow.AddHours(increment).Ticks);
            Array.Reverse(currentAsBytes);
            var bytes = new byte[16];
            Array.Copy(ticksAsBytes, 0, bytes, 0, ticksAsBytes.Length);
            Array.Copy(currentAsBytes, 0, bytes, 8, currentAsBytes.Length);
            return new Guid(bytes);
        }

        public static BigInteger ToEtagComparable(this IRealTimeInventory inventory)
        {
            return inventory.ETag.ToEtagComparable();
        }
        public static BigInteger ToEtagComparable(this Guid? eTag)
        {
            return BigInteger.Parse(eTag.ToString().Replace("-", ""), NumberStyles.AllowHexSpecifier);
        }
        public static bool IsMostRecentThan(this IRealTimeInventory @thisRealTimeInventory,IRealTimeInventory comparedToRealTimeInventory)
        {
            return thisRealTimeInventory.ETag!=null && thisRealTimeInventory.ETag.IsMostRecentThan(comparedToRealTimeInventory.ETag);
        }
        public static bool IsMostRecentThan(this Guid? thisEtag, Guid? comparedToEtag)
        {
            return (thisEtag != null && comparedToEtag != null &&(thisEtag).ToEtagComparable() > comparedToEtag.ToEtagComparable());
        }

        public static Guid? GetMostRecentEtag(this Guid? thisEtag,params Guid?[] comparedToEtag)
        {
            return comparedToEtag!=null && comparedToEtag.Length>0? comparedToEtag.OrderByDescending(x => x.ToEtagComparable()).FirstOrDefault(): thisEtag;
        }
    }
}