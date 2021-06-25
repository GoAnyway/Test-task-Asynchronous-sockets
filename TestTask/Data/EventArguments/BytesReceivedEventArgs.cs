using System.Collections.Generic;

namespace TestTask.Data.EventArguments
{
    /// <summary>
    ///     Arguments for the event of receiving a set of bytes from the server.
    /// </summary>
    public class BytesReceivedEventArgs : CommonEventArgs
    {
        public BytesReceivedEventArgs(int requestId, IEnumerable<byte> receivedBytes) 
            : base(requestId)
        {
            RequestId = requestId;
            ReceivedBytes = receivedBytes;
        }

        public IEnumerable<byte> ReceivedBytes { get; }
    }
}