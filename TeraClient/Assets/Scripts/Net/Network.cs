using System;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using Hoba.ObjectPool;

namespace Common.Net
{
    //网络库的主类，包含基本的网络功能（connect / receive / send / accept /close）
    public class Network
    {
        private const int DO_MESSAGE_INTERVAL = 10;
        public const int BUFFER_SIZE = 1024*8;  // 缓冲区大小，服务器与客户端要保持一致，不可单方面修改该值
        private const int CONNECT_TIME_OUT = 15000;//连接超时时间 毫秒

        private ExSocket _ServiceSocket = null;
        private Thread _SyncSendMessageThread = null;
        private System.Timers.Timer _ConnectTimer = null;

        private IPEndPoint _RemoteEndPoint = null;

        private DataToken _DataToken = new DataToken();

        private byte[] _BufferBlock = null;
        private SocketAsyncEventArgs _SAEA4Connect;   //处理连接的SAEA对象
        private SocketAsyncEventArgs _SAEA4Recv;      //处理接收的SAEA对象

        // 要同步发送的所有消息，集中在发送线程处理
        private readonly ThreadSafeQueue<byte[]> _CachedSendMessageQueue = new ThreadSafeQueue<byte[]>();
        // 收到的所有消息，集中在主线程处理
        private readonly ThreadSafeQueue<ConnectionEventArgs> _ConEventArgsQueue = new ThreadSafeQueue<ConnectionEventArgs>();

        private readonly ObjectPool<ConnectionEventArgs> _ConnectionEventArgsPool = new ObjectPool<ConnectionEventArgs>(10, 100, () => { return new ConnectionEventArgs(); });

        public Network()
        {
            _BufferBlock = new byte[BUFFER_SIZE];
        }

        public void Connect(string ipStr, int port)
        {
            _CachedSendMessageQueue.Clear();
            _ConEventArgsQueue.Clear();

            _RemoteEndPoint = new IPEndPoint(IPAddress.Parse(ipStr), port);
            if (_ServiceSocket != null)
            {
                _ServiceSocket.Close();
                _ServiceSocket = null;
            }
            _ServiceSocket = new ExSocket(_RemoteEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            if(_SAEA4Connect != null)
            {
                _SAEA4Connect.Dispose();
                _SAEA4Connect = null;
            }
            _SAEA4Connect = new SocketAsyncEventArgs { RemoteEndPoint = _RemoteEndPoint };
            _SAEA4Connect.Completed += OnConnectedCompleted;

            try
            {
                if(!_ServiceSocket.ConnectAsync(_SAEA4Connect))
                    ProcessConnect(_SAEA4Connect);

                if (_ConnectTimer != null)
                    _ConnectTimer.Close();

                _ConnectTimer = new System.Timers.Timer(CONNECT_TIME_OUT);
                _ConnectTimer.AutoReset = false;
                _ConnectTimer.Enabled = true;
                _ConnectTimer.Elapsed += new System.Timers.ElapsedEventHandler((object source, System.Timers.ElapsedEventArgs e) =>
                {

                    if (_ServiceSocket != null && !_ServiceSocket.Connected)
                    {
                        _ServiceSocket.Close();
                        _ServiceSocket = null;
                        var cea = _ConnectionEventArgsPool.GetObject();
                        cea.Set(EnumConnectionEventType.connect_failed, null, null, 0);
                        _ConEventArgsQueue.Enqueue(cea);
                    }
                });
            }
            catch (Exception ex)
            {
                HobaDebuger.Log(HobaText.Format("Connect Failed: {0}", ex.Message));
            }

            if (_SyncSendMessageThread != null)
            {
                _SyncSendMessageThread.Abort();
                _SyncSendMessageThread = null;
            }

            _SyncSendMessageThread = new Thread(SendMessageLoop);
            _SyncSendMessageThread.Start();
        }


        public void OnConnectedCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                var cea = _ConnectionEventArgsPool.GetObject();
                cea.Set(EnumConnectionEventType.connect_failed, null, null, 0);
                _ConEventArgsQueue.Enqueue(cea);
            }
            else
            {
                if (e.LastOperation == SocketAsyncOperation.Connect)
                    ProcessConnect(e);
                else
                    HobaDebuger.Log(HobaText.Format("Connect_Completed Error: {0}", e.LastOperation));
            }
        }

        public void OnReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            try
            {
                if (e.LastOperation == SocketAsyncOperation.Receive)
                {
                    ProcessReceive(e);
                }
                else if (e.LastOperation == SocketAsyncOperation.Send)
                {
                    
                }
                else
                {
                    HobaDebuger.Log(HobaText.Format("LastOperation is not send or receive: {0}", e.LastOperation));
                }
            }
            catch (Exception ex)
            {
                HobaDebuger.Log(HobaText.Format("OnReceiveCompleted Failed : {0}", ex.Message));
            }
        }


        private void ProcessConnect(SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
                return;

            var cea = _ConnectionEventArgsPool.GetObject();
            cea.Set(EnumConnectionEventType.connect, _ServiceSocket, null, 0);
            _ConEventArgsQueue.Enqueue(cea);

            _DataToken.Reset(true);
            
            _SAEA4Recv = new SocketAsyncEventArgs();
            _SAEA4Recv.SetBuffer(_BufferBlock, 0, BUFFER_SIZE);
            _SAEA4Recv.RemoteEndPoint = _RemoteEndPoint;
            _SAEA4Recv.Completed += OnReceiveCompleted;

            if(!_ServiceSocket.ReceiveAsync(_SAEA4Recv))
                ProcessReceive(_SAEA4Recv);

            if (_ConnectTimer != null)
                _ConnectTimer.Close();

            _SAEA4Connect.Dispose();
            _SAEA4Connect = null;
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (e.BytesTransferred == 0 || _SAEA4Recv.SocketError != SocketError.Success)
            {
                // _requestHandler为null时为Client主动关闭，_requestHandler会先行清理
                var cea = _ConnectionEventArgsPool.GetObject();
                cea.Set(EnumConnectionEventType.disconnect, _ServiceSocket, null, 0);
                _ConEventArgsQueue.Enqueue(cea);

                //对方主动关闭
                CloseSocket();
                return;
            }

            bool needPostAnother = TryMakeMessages();
            if (!needPostAnother)
            {
                CloseSocket();
                return;
            }

            if (!_ServiceSocket.ReceiveAsync(e))
                ProcessReceive(e);
        }

        private bool TryMakeMessages()
        {
            bool res = false;
            do
            {
                try
                {
                    List<byte[]> msgList = null;

                    if (!MessageProcessUtil.TryReadMessage(_DataToken, _SAEA4Recv.Buffer, _SAEA4Recv.BytesTransferred, out msgList))
                        break;

                    if (msgList != null)
                    {
                        var v = msgList.GetEnumerator();
                        while (v.MoveNext())
                        {
                            var cea = _ConnectionEventArgsPool.GetObject();
                            cea.Set(EnumConnectionEventType.recv, _ServiceSocket, v.Current, 0);
                            _ConEventArgsQueue.Enqueue(cea);
                        }
                        v.Dispose();

                        msgList.Clear();
                        res = true;
                    }
                    
                }
                catch (Exception ex)
                {
                    HobaDebuger.Log(HobaText.Format("TryReceiveMessage: {0}", ex.Message));
                }
            } while (false);

            return res;
        }

        public int GetMessageCount()
        {
            if (_ConEventArgsQueue == null) return 0;

            return _ConEventArgsQueue.Count;
        }

        public bool TryDequeueMessage(out ConnectionEventArgs entArgs)
        {
            entArgs = null;

            if (_ConEventArgsQueue == null) return false;

            return _ConEventArgsQueue.TryDequeue(out entArgs);
        }

        //将所有待发信息放到队列中，在一个线程中统一发送
        public void SendMessage(byte[] buffer)
        {
            _CachedSendMessageQueue.Enqueue(buffer);
        }

        private void SendMessageLoop()
        {
            try
            {
                while (true)
                {
                    byte[] socketSyncBytes = null;
                    _CachedSendMessageQueue.TryDequeue(out socketSyncBytes);

                    if (socketSyncBytes != null && _ServiceSocket != null)
                    {
                        int len = socketSyncBytes.Length;
                        int sendLen = _ServiceSocket.Send(socketSyncBytes, len);
                        if (len != sendLen)
                            HobaDebuger.Log("SendMessageLoop Failed: len != sendLen ");
                    }
                    else
                    {
                        Thread.Sleep(DO_MESSAGE_INTERVAL);
                    }
                }
            }
            catch (ThreadAbortException e)
            {
                HobaDebuger.LogFormat("SendMessageLoop Failed bcz {0}", e.ToString());
            }
        }

        private void CloseSocket()
        {
            var cea = _ConnectionEventArgsPool.GetObject();
            cea.Set(EnumConnectionEventType.close, _ServiceSocket, null, 0);
            _ConEventArgsQueue.Enqueue(cea);

            if (_ServiceSocket != null)
            {
                _ServiceSocket.Close();
				_ServiceSocket = null;
            }

            if(_DataToken != null)
                _DataToken.Reset(true);

            if (_SAEA4Connect != null)
            {
                _SAEA4Connect.Dispose();
                _SAEA4Connect = null;
            }

            if (_SAEA4Recv != null)
            {
                _SAEA4Recv.Dispose();
                _SAEA4Recv = null;
            }
        }

        public void Release()
        {
            CloseSocket();

            _CachedSendMessageQueue.Clear();
            _ConEventArgsQueue.Clear();

            _BufferBlock = null;

            if (_SyncSendMessageThread != null)
            {
                //_SyncSendMessageThread.Interrupt();
                _SyncSendMessageThread.Abort();
                _SyncSendMessageThread = null;
            }

            if (_ConnectTimer != null)
            {
                _ConnectTimer.Stop();
                _ConnectTimer.Close();
            }

            _ServiceSocket = null;
            _DataToken = null;
        }
    }
}
