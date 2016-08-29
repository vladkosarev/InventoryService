using System;
using System.Collections.Generic;

namespace InventoryService.Storage
{
    public class StorageOperationResult
    {
        public bool IsSuccessful { set; get; }
        public AggregateException Errors { set; get; }
    }

    public class StorageOperationResult<T>: StorageOperationResult
    {
        public StorageOperationResult(T result)
        {
            Result = result;
            IsSuccessful = true;

        }
        public T Result { private set; get; }
    }
}