using System;

namespace Common.Net
{
    //每个连接相关联的用户数据，用来异步的数据传递
    public class DataToken
    {
        //[消息长度（int）][消息主体]
        public byte[] ByteArrayForMessage;  //消息主体缓存
        public byte[] ByteArrayForPrefix;   //消息长度缓存
        public int MessageDoneNum = 0;      //已经接收的主体的长度
        public int PrefixDoneNum = 0;       //要接收的消息长度的长度
        public int MessageLength = 0;       //消息长度：ByteArrayForPrefix转化为int
        public int BufferSkipOffset = 0;    //处理当前接收到的数据时，要跳过的偏移

        public DataToken()
        {
            ByteArrayForPrefix = new byte[4];
        }

        public void Reset(bool isSkip)
        {
            ByteArrayForMessage = null;
            Array.Clear(ByteArrayForPrefix, 0, ByteArrayForPrefix.Length);
            MessageDoneNum = 0;
            PrefixDoneNum = 0;
            MessageLength = 0;
            if(isSkip)
                BufferSkipOffset = 0;
        }
    }
}
