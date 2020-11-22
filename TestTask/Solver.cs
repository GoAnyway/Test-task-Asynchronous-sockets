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
        private readonly Dictionary<int, List<byte>> _dictionary = new Dictionary<int, List<byte>>();

        private readonly object _dictionaryLock = new object();

        /// <summary>
        ///     Number of <see cref="CustomTcpClient" /> running simultaneously.
        ///     Recommended Max Amount: 100.
        /// </summary>
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(100);

        private readonly Stopwatch _stopwatch = new Stopwatch();

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
            for (var idx = 1; idx <= N; idx++)
            {
                lock (_dictionaryLock)
                {
                    _dictionary[idx] = new List<byte>();
                }

                var copied = idx;
                Task.Run(() =>
                {
                    _semaphore.Wait();
                    RequestServerValue(copied);
                });
            }

            while (true)
            {
                lock (_dictionaryLock)
                {
                    if (_done >= N)
                    {
                        var values = _dictionary.Values.Select(_ =>
                            int.Parse(Encoding.GetEncoding("koi8-r").GetString(_.ToArray())));
                        OnAllDataReceived(new AllDataReceivedEventArgs(values));

                        return;
                    }
                }

                Thread.Sleep(50);
            }
        }

        private void RequestServerValue(int copied)
        {
            var client = new CustomTcpClient(copied);
            client.BytesReceived += Client_BytesReceived;
            client.ErrorReceived += Client_ErrorReceived;
            client.ResponseReceived += Client_ResponseReceived;
            client.StartClient();
        }

        private void Client_ResponseReceived(object sender, CommonEventArgs e)
        {
            string response;
            lock (_dictionaryLock)
            {
                ++_done;
                response = Encoding.GetEncoding("koi8-r").GetString(_dictionary[e.Request].ToArray());
            }

            Console.WriteLine(
                $"{_stopwatch.Elapsed} Task #{e.Request} completed, response: {response}, done: {_done}/{N}");
            _semaphore.Release();
        }

        private void Client_ErrorReceived(object sender, ErrorEventArgs e)
        {
            Console.WriteLine($"{_stopwatch.Elapsed} Re-firing task #{e.Request}");
            lock (_dictionaryLock)
            {
                _dictionary[e.Request] = new List<byte>();
            }

            RequestServerValue(e.Request);
        }

        private void Client_BytesReceived(object sender, BytesReceivedEventArgs e)
        {
            lock (_dictionaryLock)
            {
                foreach (var b in e.ReceivedBytes)
                {
                    if (b < '0' || b > '9') continue;
                    _dictionary[e.Request].Add(b);
                }
            }
        }

        protected virtual void OnAllDataReceived(AllDataReceivedEventArgs e) => AllDataReceived?.Invoke(this, e);
    }
}