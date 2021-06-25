using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestTask.EventArguments;

namespace TestTask
{
    /// <summary>
    ///     Class for solving the main problem.
    /// </summary>
    public class Solver
    {
        /// <summary>
        ///     The number of required requests to the server.
        /// </summary>
        private const int N = 2018;

        /// <summary>
        ///     Server response store.
        /// </summary>
        private readonly Dictionary<int, List<byte>> _responses = new();

        /// <summary>
        ///     Number of <see cref="CustomTcpClient" /> running simultaneously.
        ///     <para> Recommended Max Amount: 100. </para>
        /// </summary>
        private readonly SemaphoreSlim _semaphore = new(100);

        private readonly Stopwatch _stopwatch = new();

        /// <summary>
        ///     Number of responses received from the server.
        /// </summary>
        private int _done;

        /// <summary>
        ///     Event called when all responses from the server have been received.
        /// </summary>
        public event EventHandler<AllDataReceivedEventArgs> AllDataReceived;

        public void Start()
        {
            _stopwatch.Start();
            InitResponses();
            InitRequests();

            while (true)
            {
                if (Interlocked.CompareExchange(ref _done, N, N) == N)
                {
                    var values = _responses.Values.Select(_ =>
                        int.Parse(Encoding.GetEncoding("koi8-r").GetString(_.ToArray())));
                    OnAllDataReceived(new AllDataReceivedEventArgs(values));

                    return;
                }

                Thread.Sleep(50);
            }
        }

        private void InitRequests()
        {
            Task.Run(() =>
            {
                Parallel.For(1, N + 1, request =>
                {
                    _semaphore.Wait();
                    RequestServerValue(request);
                });
            });
        }

        private void InitResponses()
        {
            // We need to do this to avoid using ConcurrentDictionary or locks.
            for (var idx = 1; idx <= N; idx++) _responses[idx] = new List<byte>();
        }

        private void RequestServerValue(int request)
        {
            var client = new CustomTcpClient(request);
            client.BytesReceived += Client_BytesReceived;
            client.ErrorReceived += Client_ErrorReceived;
            client.ResponseReceived += Client_ResponseReceived;
            client.StartClient();
        }

        private void Client_ResponseReceived(object sender, CommonEventArgs e)
        {
            Interlocked.Increment(ref _done);
            var response = Encoding.GetEncoding("koi8-r").GetString(_responses[e.Request].ToArray());

            Console.WriteLine(
                $"{_stopwatch.Elapsed} Task #{e.Request} completed, response: {response}, done: {_done}/{N}");
            _semaphore.Release();
        }

        private void Client_ErrorReceived(object sender, ErrorEventArgs e)
        {
            Console.WriteLine($"{_stopwatch.Elapsed} Re-firing task #{e.Request}");
            _responses[e.Request] = new List<byte>();

            RequestServerValue(e.Request);
        }

        private void Client_BytesReceived(object sender, BytesReceivedEventArgs e)
        {
            foreach (var @byte in e.ReceivedBytes)
            {
                if (@byte < '0' || @byte > '9') continue;
                _responses[e.Request].Add(@byte);
            }
        }

        protected virtual void OnAllDataReceived(AllDataReceivedEventArgs e) => AllDataReceived?.Invoke(this, e);
    }
}