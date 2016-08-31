namespace InventoryService.Messages.Response
{
    public interface ICompletedMessage
    {
         string ProductId { get;  }
         int Quantity { get;  }
         int Reserved { get;  }
         int Holds { get;  }
         bool Successful { get; set; }
        
    }
}