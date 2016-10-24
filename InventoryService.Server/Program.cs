using System;

namespace InventoryService.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            new InventoryServiceApplication().Start((a, s) =>
            {
                Console.WriteLine("Server started ...");
            }
            );
        }
    }
}