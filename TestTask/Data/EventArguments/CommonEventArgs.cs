using System;

namespace TestTask.Data.EventArguments
{
    /// <summary>
    ///     Common Event Arguments.
    /// </summary>
    public class CommonEventArgs : EventArgs
    {
        public CommonEventArgs(int requestId)
        {
            RequestId = requestId;
        }

        public int RequestId { get; protected set; }
    }
}