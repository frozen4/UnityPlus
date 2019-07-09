using System;

namespace Common.Net
{
    // 节点网络类型
    public enum EnumNodeNetType
    {
        Error = 0,
        Service = 1,
        Client = 2,
    }

    public delegate void ReceiveMessageHandler(ExSocket exSocket, byte[] data);
    public class NetNode
    {
        public Guid HashCode;
        /// <summary>
        /// 消息处理函数
        /// </summary>
        public event ReceiveMessageHandler HandleReceivedMessage;
        public NetNode()
        {
            HashCode = Guid.NewGuid();
        }

        public virtual void Start(string ipStr, int port) { }
        public virtual void DoNetMessage() { }

        public virtual void OnProtocolError(ExSocket exSocket) { }
        public virtual void OnReceiveMessage(ExSocket exSocket, byte[] data)
        {
            if(HandleReceivedMessage != null)
                HandleReceivedMessage(exSocket, data);
        }
    }
}
