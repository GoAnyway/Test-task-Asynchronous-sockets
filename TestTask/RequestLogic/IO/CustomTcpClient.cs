using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TestTask.Data.EventArguments;

namespace TestTask.RequestLogic.IO
{
    /// <summary>
    ///     Custom implementation of a TCP-client based on working with asynchronous sockets.
    /// </summary>
    public class CustomTcpClient
    {
        private readonly int _requestId;

        public CustomTcpClient(int requestId)
        {
            _requestId = requestId;
        }

        public void StartClient()
        {
            var ip = IPAddress.Parse(Host);
            var endpoint = new IPEndPoint(ip, Port);
            using var socket = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {
                ReceiveTimeout = 500,
                SendTimeout = 500
            };

            try
            {
                socket.BeginConnect(endpoint, ConnectCallback, socket);
                _connectDone.WaitOne();
                Send(socket, $"{_requestId}\n");
                _sendDone.WaitOne();
                Receive(socket);
                _receiveDone.WaitOne();
            }
            catch (Exception e)
            {
                OnErrorReceived(new ErrorEventArgs(_requestId, e));
            }
        }

        private void ConnectCallback(IAsyncResult result)
        {
            try
            {
                var socket = (Socket) result.AsyncState;
                socket?.EndConnect(result);
                _connectDone.Set();
            }
            catch (Exception e)
            {
                OnErrorReceived(new ErrorEventArgs(_requestId, e));
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
                OnErrorReceived(new ErrorEventArgs(_requestId, e));
            }
        }

        private void ReceiveCallback(IAsyncResult result)
        {
            var state = (StateObject) result.AsyncState;
            try
            {
                var socket = state.WorkSocket;
                var bytesRead = socket.EndReceive(result);
                if (bytesRead > 0)
                {
                    var actuallyReceivedBytes = state.Buffer.Take(bytesRead).ToArray();
                    OnBytesReceived(new BytesReceivedEventArgs(_requestId, actuallyReceivedBytes));
                    if (actuallyReceivedBytes[bytesRead - 1] == '\r')
                    {
                        OnResponseReceived(new CommonEventArgs(_requestId));
                        _receiveDone.Set();
                    }
                    else
                    {
                        socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0, ReceiveCallback, state);
                    }
                }
                else
                {
                    _receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                OnErrorReceived(new ErrorEventArgs(_requestId, e));
            }
        }

        private void Send(Socket socket, string data)
        {
            var byteData = Encoding.GetEncoding("koi8-r").GetBytes(data);
            socket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, SendCallback, socket);
        }

        private void SendCallback(IAsyncResult result)
        {
            try
            {
                var socket = (Socket) result.AsyncState;
                socket.EndSend(result);
                _sendDone.Set();
            }
            catch (Exception e)
            {
                OnErrorReceived(new ErrorEventArgs(_requestId, e));
            }
        }

        protected virtual void OnBytesReceived(BytesReceivedEventArgs e) => BytesReceived?.Invoke(this, e);

        protected virtual void OnErrorReceived(ErrorEventArgs e) => ErrorReceived?.Invoke(this, e);

        protected virtual void OnResponseReceived(CommonEventArgs e) => ResponseReceived?.Invoke(this, e);

        #region Events

        public event EventHandler<BytesReceivedEventArgs> BytesReceived;
        public event EventHandler<ErrorEventArgs> ErrorReceived;
        public event EventHandler<CommonEventArgs> ResponseReceived;

        #endregion

        #region Thread synchronization events

        private readonly ManualResetEvent _connectDone = new(false);
        private readonly ManualResetEvent _sendDone = new(false);
        private readonly ManualResetEvent _receiveDone = new(false);

        #endregion

        #region Constants for connecting to the server

        private const int Port = 2012;
        private const string Host = "88.212.241.115";

        #endregion
    }
}