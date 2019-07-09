using System;
using Hoba.ObjectPool;

namespace Common.Net
{

    public delegate void ConnectionEventHandler(Network netManager, ConnectionEventArgs e);

    public enum EnumConnectionEventType
    {
        unknown,
        accept,
        connect,
        disconnect,
        connect_failed,
        recv,
        close,
    }

    //连接事件的参数
    public class ConnectionEventArgs : PooledObject
    {
        public EnumConnectionEventType EventType { get; set; }
        public ExSocket Exsocket { get; set; }
        public int PrtcId { get; set; }
        public byte[] Message { get; set; }

        public ConnectionEventArgs()
        {
            EventType = EnumConnectionEventType.unknown;
            Exsocket = null;
            Message = null;
            PrtcId = 0;
        }

        public void Set(EnumConnectionEventType t, ExSocket e, byte[] d, int l)
        {
            EventType = t;
            Exsocket = e;
            Message = d;
            PrtcId = 0;
        }

        protected override void OnResetState()
        {
            Set(EnumConnectionEventType.unknown, null, null, 0);
        }
    }
}
