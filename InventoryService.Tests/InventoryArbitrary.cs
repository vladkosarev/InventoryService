using FsCheck;
using InventoryService.Messages.Models;
using System;

namespace InventoryService.Tests
{
    public static class InventoryArbitrary
    {
        public static Arbitrary<RealTimeInventory> Inventories()
        {
            var genInventories = from name in Arb.Generate<Guid>()
                                 from quantity in Arb.Generate<uint>()
                                 from reservations in Gen.Choose(0, (int)quantity)
                                 from holds in Gen.Choose(0, (int)quantity)
                                 select new RealTimeInventory(name.ToString(), (int)quantity, reservations, holds);
            return genInventories.ToArbitrary();
        }
    }
}