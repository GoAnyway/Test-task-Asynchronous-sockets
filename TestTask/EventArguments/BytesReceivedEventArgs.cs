using System.Collections.Generic;

namespace TestTask.EventArguments
{
    /// <summary>
    ///     Arguments for the event of receiving a set of bytes from the server.
    /// </summary>
    public class BytesReceivedEventArgs : CommonEventArgs
    {
        public BytesReceivedEventArgs(int request, IEnumerable<byte> receivedBytes) : base(request)
        {
            Request = request;
            ReceivedBytes = receivedBytes;
        }

        public IEnumerable<byte> ReceivedBytes { get; }
    }
}