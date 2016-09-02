using InventoryService.Messages.Request;
using System.Collections.Generic;

namespace InventoryService.Messages.Response
{
    public class GetRemovedProductCompletedMessage
    {
        public GetRemovedProductCompletedMessage(List<RemoveProductMessage> removedProducts)
        {
            RemovedProducts = removedProducts;
        }

        public List<RemoveProductMessage> RemovedProducts { private set; get; }
    }
}