using System;

namespace InventoryService.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            new InventoryServiceApplication().Start();
            Console.WriteLine("Server started ...");
        }
    }
}