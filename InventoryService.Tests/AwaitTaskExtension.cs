using System.Threading.Tasks;
using InventoryService.Messages.Response;

namespace InventoryService.Tests
{
    public static class AwaitTaskExtension
    {
        public static IInventoryServiceCompletedMessage WaitAndGetOperationResult(this Task<IInventoryServiceCompletedMessage> task)
        {
            task.ConfigureAwait(false);
            Task.WaitAll(task);
            return task.Result;
        }
    }
}