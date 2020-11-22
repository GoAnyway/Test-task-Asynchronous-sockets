using System;

namespace TestTask.EventArguments
{
    /// <summary>
    ///     Common Event Arguments.
    /// </summary>
    public class CommonEventArgs : EventArgs
    {
        public CommonEventArgs(int request)
        {
            Request = request;
        }

        public int Request { get; protected set; }
    }
}