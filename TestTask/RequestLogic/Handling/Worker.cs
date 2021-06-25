using System.Linq;
using System.Threading;
using TestTask.Data.EventArguments;
using TestTask.Data.RequestLogic;
using TestTask.RequestLogic.IO;

namespace TestTask.RequestLogic.Handling
{
    /// <summary>
    /// Handles available requests from the <see cref="RequestPool"/>.
    /// </summary>
    public class Worker
    {
        private static long _activeWorkersCount;
        private readonly RequestPool _requestPool;

        public Worker(RequestPool requestPool)
        {
            _requestPool = requestPool;
        }

        public static long ActiveWorkersCount => Interlocked.Read(ref _activeWorkersCount);

        public void StartWork()
        {
            var request = _requestPool.TakeAvailableRequest();
            if (request.State == RequestState.None) return;

            IncrementActiveWorkersCount();
            var client = new CustomTcpClient(request.Id);
            client.BytesReceived += Client_BytesReceived;
            client.ErrorReceived += Client_ErrorReceived;
            client.ResponseReceived += Client_ResponseReceived;
            client.StartClient();
        }

        private void Client_ResponseReceived(object sender, CommonEventArgs e)
        {
            _requestPool.ChangeRequestState(e.RequestId, RequestState.Done);
            DecrementActiveWorkersCount();
        }

        private void Client_ErrorReceived(object sender, CommonEventArgs e)
        {
            _requestPool.ChangeRequestState(e.RequestId, RequestState.Failed);
            DecrementActiveWorkersCount();
        }

        private void Client_BytesReceived(object sender, BytesReceivedEventArgs e)
        {
            var numbers = e.ReceivedBytes.Where(_ => _ >= '0' && _ <= '9');
            _requestPool.UpdateResponse(e.RequestId, numbers);
        }

        private void IncrementActiveWorkersCount() => Interlocked.Increment(ref _activeWorkersCount);
        private void DecrementActiveWorkersCount() => Interlocked.Decrement(ref _activeWorkersCount);
    }
}