using System;

namespace TestTask.EventArguments
{
    /// <summary>
    ///     Arguments of the event that is called
    ///     if an error was received while receiving a response from the server by the client
    /// </summary>
    public class ErrorEventArgs : CommonEventArgs
    {
        public ErrorEventArgs(int request, Exception e) : base(request)
        {
            Exception = e;
            Request = request;
        }

        public Exception Exception { get; }
    }
}