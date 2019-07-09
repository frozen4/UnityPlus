using System.Net;

namespace Common.Net
{
    public enum EVENT
    {
        NONE = 0,

        CONNECTED = 1,
        DISCONNECTED = 2,
        ACCEPTED = 3,
        CLOSED = 4,
        CONNECT_FAILED = 5,
    }

    //Unity客户端专用：发送机制从异步改为了同步
    public class NetClient : NetNode
    {
        public delegate void OnEvent(EVENT eventId);

        public OnEvent OnGatewayEvent = null;
        protected Network _Network = null;
        private string _IP = null;
        public NetClient()
        {
            //_remoteEndPoint = new IPEndPoint(IPAddress.Parse(ipStr), port);
            _Network = new Network();
        }

        public override void Start(string ipStr, int port)
        {
            if(_Network == null) return;
            _IP = ipStr;
            _Network.Connect(ipStr, port);
        }

        public void OnConnected(ConnectionEventArgs e)
        {
            //_serviceSocket = e.Exsocket;
#if UNITY_EDITOR || UNITY_STANDALONE
            HobaDebuger.LogFormat(HobaText.Format("{0} Connected.", _IP));
#endif
            if (OnGatewayEvent != null)
                OnGatewayEvent(EVENT.CONNECTED);
        }

        public void OnDisconnected(ConnectionEventArgs e)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            HobaDebuger.LogFormat(HobaText.Format("{0} Disconnected.", _IP));
#endif
            if (OnGatewayEvent != null)
                OnGatewayEvent(EVENT.DISCONNECTED);
        }

        public void OnClosed(ConnectionEventArgs e)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            HobaDebuger.LogFormat(HobaText.Format("{0} Closed.", _IP));
#endif
            if (OnGatewayEvent != null)
                OnGatewayEvent(EVENT.CLOSED);
        }

        public void OnConnectFailed(ConnectionEventArgs e)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            HobaDebuger.LogFormat(HobaText.Format("{0} ConnectFailed.", _IP));
#endif
            if (OnGatewayEvent != null)
                OnGatewayEvent(EVENT.CONNECT_FAILED);
        }

        //protobuf序列化（调用.serialize()方法）后的裸数据
        public void SendSync(byte[] data)
        {
            if (_Network != null)
                _Network.SendMessage(data);
        }

        public override void DoNetMessage()
        {
            int count = _Network.GetMessageCount();
            for (int i = 0; i < count; i++)
            {
                ConnectionEventArgs entArgs = null;
                if (!_Network.TryDequeueMessage(out entArgs) || entArgs == null)
                    continue;

                switch (entArgs.EventType)
                {
                    case EnumConnectionEventType.accept:
                        break;
                    case EnumConnectionEventType.connect:
                        OnConnected(entArgs); break;
                    case EnumConnectionEventType.disconnect:
                        OnDisconnected(entArgs); break;
                    case EnumConnectionEventType.connect_failed:
                        OnConnectFailed(entArgs); break;
                    case EnumConnectionEventType.recv:
                        OnReceiveMessage(entArgs.Exsocket, entArgs.Message); break;
                    case EnumConnectionEventType.close:
                        OnClosed(entArgs); break;
                }
                entArgs.Message = null;
                entArgs.Dispose();
            }
        }

        //主动关闭连接
        public void Release()
        {
            _IP = null;

            if (_Network != null)
                _Network.Release();
        }
    }
}
