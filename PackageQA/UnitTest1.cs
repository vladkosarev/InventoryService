using InventoryService.AkkaInMemoryServer;
using InventoryService.Messages.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PackageQA
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (var server = new InventoryServiceAkkaInMemory(new RealTimeInventory("sample", 10, 0, 0)))
            {
                var t = server.GetInventoryAsync("sample").Result;
                Console.WriteLine(t);
            }
        }
    }
}