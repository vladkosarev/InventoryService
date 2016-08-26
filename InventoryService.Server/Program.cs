using System;

namespace InventoryService.Server
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                new InventoryServiceServer().StartServer();
            }
            catch (Exception)
            {
                
              
            }
            Console.ReadLine();
        }
    }
}