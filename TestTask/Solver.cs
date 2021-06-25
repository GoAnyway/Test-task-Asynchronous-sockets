using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using TestTask.Data.EventArguments;
using TestTask.RequestLogic.Handling;
using DataReceivedEventArgs = TestTask.Data.EventArguments.DataReceivedEventArgs;

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
        private readonly Dictionary<int, int> _responses = new();

        /// <summary>
        ///     Number of <see cref="Worker" /> running simultaneously.
        ///     <para> Recommended Max Amount: 100. </para>
        /// </summary>
        private const int MaxWorkersCount = 64;

        /// <summary>
        ///     Number of responses received from the server.
        /// </summary>
        private int _done;

        private readonly Stopwatch _stopwatch = new();

        public Solver()
        {
            // We need to do this to avoid using ConcurrentDictionary or locks.
            for (var idx = 1; idx <= N; idx++) _responses[idx] = 0;
        }

        /// <summary>
        ///     Event called when all responses from the server have been received.
        /// </summary>
        public event EventHandler<AllDataReceivedEventArgs> AllDataReceived;

        public void Start()
        {
            _stopwatch.Start();
            var requestPool = new RequestPool(N);
            requestPool.DataReceived += DataReceived;

            while (true)
            {
                if (Interlocked.CompareExchange(ref _done, N, N) == N)
                {
                    OnAllDataReceived(new AllDataReceivedEventArgs(_responses.Values));
                    return;
                }

                if (Worker.ActiveWorkersCount != MaxWorkersCount)
                {
                    Task.Run(() => new Worker(requestPool).StartWork());
                }

                Thread.Sleep(1);
            }
        }

        private void DataReceived(object sender, DataReceivedEventArgs e)
        {
            Interlocked.Increment(ref _done);
            _responses[e.RequestId] = e.Data;
            Console.WriteLine(
                $"{_stopwatch.Elapsed} Request #{e.RequestId} completed, response: {e.Data}, done: {_done}/{N}");
        }

        protected virtual void OnAllDataReceived(AllDataReceivedEventArgs e) => AllDataReceived?.Invoke(this, e);
    }
}