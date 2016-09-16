namespace InventoryService.Messages
{
    public class RealTimeInventoryException
    {
        public string ErrorMessage { get; set; }
    }

    //public class RealTimeInventoryException : Exception
    //{
    //    public RealTimeInventoryException(string errorMessage, ErrorType errorType, DateTime? occuredAt, Exception exceptionThrown)
    //    {
    //        ErrorMessage = errorMessage;
    //        ErrorType = errorType;
    //        OccuredAt = occuredAt;
    //        ExceptionThrown = exceptionThrown;
    //    }

    //    public Exception ExceptionThrown { get; private set; }
    //    public string ErrorMessage { get; private set; }
    //    public ErrorType ErrorType { get; private set; }
    //    public DateTime? OccuredAt { get; private set; }
    //}
}

/*
 using System;
using System.Collections.Generic;

namespace InventoryService.Messages
{
    public class RealTimeInventoryException : Exception
    {
        public RealTimeInventoryException(string errorMessage, ErrorType errorType, DateTime? occuredAt, Exception exceptionThrown)
        {
            ErrorMessage = new List<string> {errorMessage};
            ErrorType = errorType;
            OccuredAt = occuredAt;
            ExceptionThrown = exceptionThrown;
        }

        public Exception ExceptionThrown { get; private set; }
        public List<string> ErrorMessage { get; private set; }
        public ErrorType ErrorType { get; private set; }
        public DateTime? OccuredAt { get; private set; }
    }
}
     */