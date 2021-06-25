using System.Net.Sockets;

namespace TestTask.RequestLogic.IO
{
    /// <summary>
    ///     The state object of communication with the server via <see cref="Socket" />.
    /// </summary>
    public class StateObject
    {
        public const int BufferSize = 8 * 1024;

        public StateObject(Socket socket)
        {
            WorkSocket = socket;
        }

        public Socket WorkSocket { get; }
        public byte[] Buffer { get; } = new byte[BufferSize];
    }
}