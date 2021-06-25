using System;

namespace TestTask.Data.EventArguments
{
    /// <summary>
    ///     Arguments of the event that is called
    ///     if an error was received while receiving a response from the server by the client
    /// </summary>
    public class ErrorEventArgs : CommonEventArgs
    {
        public ErrorEventArgs(int requestId, Exception e) : base(requestId)
        {
            Exception = e;
            RequestId = requestId;
        }

        public Exception Exception { get; }
    }
}