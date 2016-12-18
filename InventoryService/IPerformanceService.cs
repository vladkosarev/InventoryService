namespace InventoryService
{
    public interface IPerformanceService
    {
        void Init();

        void PrintMetrics();

        void Increment(string counter);
    }
}