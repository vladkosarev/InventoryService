using System.Collections.Generic;
using InventoryService.Messages.Request;

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