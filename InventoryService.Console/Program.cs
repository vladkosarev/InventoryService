namespace InventoryService.Console
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            new SampleClientClass().StartSampleClientAsync();
            System.Console.ReadLine();
        }
    }
}