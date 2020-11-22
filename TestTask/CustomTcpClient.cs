using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TestTask.EventArguments;

namespace TestTask
{
    /// <summary>
    ///     Custom implementation of a tcp-client based on working with asynchronous sockets.
    /// </summary>
    public class CustomTcpClient
    {
        private readonly int _request;

        public CustomTcpClient(int request)
        {
            _request = request;
        }

        public void StartClient()
        {
            var ip = IPAddress.Parse(Host);
            var endpoint = new IPEndPoint(ip, Port);
            using var client = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = 500,
                SendTimeout = 500
            };

            try
            {
                client.BeginConnect(endpoint, ConnectCallback, client);
                _connectDone.WaitOne();
                Send(client, $"{_request}\n");
                _sendDone.WaitOne();
                Receive(client);
                _receiveDone.WaitOne();
            }
            catch (Exception e)
            {
                OnErrorReceived(new ErrorEventArgs(_request, e));
            }
            finally
            {
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket) ar.AsyncState;
                client?.EndConnect(ar);
                _connectDone.Set();
            }
            catch (Exception e)
            {
                OnErrorReceived(new ErrorEventArgs(_request, e));
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                var state = new StateObject(client);
                client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
            }
            catch (Exception e)
            {
                OnErrorReceived(new ErrorEventArgs(_request, e));
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            var state = (StateObject) ar.AsyncState;
            try
            {
                var client = state.WorkSocket;
                var bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    var actuallyReceivedBytes = state.Buffer.AsSpan(0, bytesRead).ToArray();
                    OnByteReceived(new BytesReceivedEventArgs(_request, actuallyReceivedBytes));
                    if (actuallyReceivedBytes.Any(_ => _ == 13))
                    {
                        OnResponseReceived(new CommonEventArgs(_request));
                        _receiveDone.Set();
                    }
                    else
                    {
                        client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
                    }
                }
                else
                {
                    _receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                OnErrorReceived(new ErrorEventArgs(_request, e));
            }
        }

        private void Send(Socket client, string data)
        {
            var byteData = Encoding.GetEncoding("koi8-r").GetBytes(data);
            client.BeginSend(byteData, 0, byteData.Length, 0, SendCallback, client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                var client = (Socket) ar.AsyncState;
                client.EndSend(ar);
                _sendDone.Set();
            }
            catch (Exception e)
            {
                OnErrorReceived(new ErrorEventArgs(_request, e));
            }
        }

        protected virtual void OnByteReceived(BytesReceivedEventArgs e) => BytesReceived?.Invoke(this, e);

        protected virtual void OnErrorReceived(ErrorEventArgs e) => ErrorReceived?.Invoke(this, e);

        protected virtual void OnResponseReceived(CommonEventArgs e) => ResponseReceived?.Invoke(this, e);

        #region Events

        public event EventHandler<BytesReceivedEventArgs> BytesReceived;
        public event EventHandler<ErrorEventArgs> ErrorReceived;
        public event EventHandler<CommonEventArgs> ResponseReceived;

        #endregion

        #region Thread synchronization events

        private readonly ManualResetEvent _connectDone = new ManualResetEvent(false);
        private readonly ManualResetEvent _sendDone = new ManualResetEvent(false);
        private readonly ManualResetEvent _receiveDone = new ManualResetEvent(false);

        #endregion

        #region Constants for connecting to the server

        private const int Port = 2012;
        private const string Host = "88.212.241.115";

        #endregion
    }
}