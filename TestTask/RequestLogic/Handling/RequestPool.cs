using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using TestTask.Data.EventArguments;
using TestTask.Data.RequestLogic;

namespace TestTask.RequestLogic.Handling
{
    /// <summary>
    /// Storage of available requests.
    /// </summary>
    public class RequestPool
    {
        private readonly ConcurrentQueue<Request> _availableRequests = new();
        private readonly Dictionary<int, List<byte>> _tempResponses = new();

        public RequestPool(int requestsCount)
        {
            for (var idx = 1; idx <= requestsCount; idx++)
            {
                _availableRequests.Enqueue(new Request(idx, RequestState.None));
                // We need to do this to avoid using ConcurrentDictionary or locks.
                _tempResponses[idx] = new List<byte>();
            }
        }

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public Request TakeAvailableRequest()
        {
            if (_availableRequests.TryDequeue(out var request))
            {
                request.State = RequestState.Taken;
                return request;
            }

            return new Request(0, RequestState.None);
        }

        public void UpdateResponse(int requestId, IEnumerable<byte> response) =>
            _tempResponses[requestId].AddRange(response);

        public void ChangeRequestState(int requestId, RequestState state)
        {
            switch (state)
            {
                case RequestState.Failed:
                    MarkRequestAsFailed(requestId);
                    break;
                case RequestState.Done:
                    MarkRequestAsDone(requestId);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        private void MarkRequestAsDone(int requestId)
        {
            var data = int.Parse(Encoding.GetEncoding("koi8-r").GetString(_tempResponses[requestId].ToArray()));
            OnDataReceived(new DataReceivedEventArgs(requestId, data));
            _tempResponses.Remove(requestId);
        }

        private void MarkRequestAsFailed(int requestId)
        {
            Console.WriteLine($"An error occurred while processing the request #{requestId}");
            _tempResponses[requestId] = new List<byte>();
            _availableRequests.Enqueue(new Request(requestId, RequestState.None));
        }

        protected virtual void OnDataReceived(DataReceivedEventArgs e) => DataReceived?.Invoke(this, e);
    }
}