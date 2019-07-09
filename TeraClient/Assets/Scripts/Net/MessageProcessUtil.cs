using System;
using System.Collections.Generic;

namespace Common.Net
{
    //基本消息分包处理类，仅分离出指定长度的数据包
    //[包体的长度][协议ID][协议内容]
    //[   32位   ][ 32位 ][....]
//     public struct Message
//     {
//         public int Id;
//         public byte[] Data;
// 
//         public Message(int id, byte[] data)
//         {
//             Id = id;
//             Data = data;
//         }
//     }

    public class MessageProcessUtil
    {

        private static List<byte[]> _MessageList = new List<byte[]>();

        public static bool TryReadMessage(DataToken dataToken, byte[] buffer, int length, out List<byte[]> messageList)
        {
            int remainingBytesToProcess = length;
            bool needPostAnother = true;

            messageList = null;
            _MessageList.Clear();

            do
            {
                if (dataToken.PrefixDoneNum < 4)
                {
                    remainingBytesToProcess = HandlePrefix(buffer, dataToken, remainingBytesToProcess);
                    if (dataToken.PrefixDoneNum == 4 && (dataToken.MessageLength > 10 * 1024 * 1024 || dataToken.MessageLength < 0 ))
                    {
                        //包长度出错
                        needPostAnother = false;
                        break;
                    }
                    if (remainingBytesToProcess == 0)  break;
                }
                else
                {
                    // TODO: remainingBytesToProcess = HandlePrtcId();
                }

                remainingBytesToProcess = HandleMessage(buffer, dataToken, remainingBytesToProcess);
                if(dataToken.MessageDoneNum == dataToken.MessageLength )//获得完整消息包
                {
                    _MessageList.Add(dataToken.ByteArrayForMessage);
                    if(remainingBytesToProcess != 0)
                        dataToken.Reset(false);
                }
            }
            while (remainingBytesToProcess != 0);

            messageList = _MessageList;

            if (needPostAnother)
            {
                //如果刚好处理完完整的包，则dataToken完全重置,否则仅重置偏移量
                if( dataToken.PrefixDoneNum == 4 && dataToken.MessageDoneNum == dataToken.MessageLength )
                    dataToken.Reset(true);
                else
                    dataToken.BufferSkipOffset = 0;
            }
            return needPostAnother;
        }
        
        private static int HandlePrefix(byte[] buffer, DataToken dataToken , int remaimingBytesToProcess)
        {
            int headerLen = 4 - dataToken.PrefixDoneNum;
            var offset = dataToken.BufferSkipOffset;

            if (remaimingBytesToProcess >= headerLen )
            {
                for (int i = 0; i < headerLen; i++ )
                {
                    dataToken.ByteArrayForPrefix[dataToken.PrefixDoneNum + i] = buffer[offset + i];
                }
                remaimingBytesToProcess = remaimingBytesToProcess - headerLen;
                dataToken.PrefixDoneNum = 4;
                dataToken.MessageLength = BitConverter.ToInt32(dataToken.ByteArrayForPrefix, 0);
                dataToken.BufferSkipOffset += headerLen;
            }
            else
            {
                for(int i = 0; i < remaimingBytesToProcess; i++ )
                {
                    dataToken.ByteArrayForPrefix[dataToken.PrefixDoneNum + i] = buffer[offset + i];
                }
                dataToken.PrefixDoneNum += remaimingBytesToProcess;
                remaimingBytesToProcess = 0;
            }
            return remaimingBytesToProcess;
        }

        private static int HandlePrtcId(byte[] buffer, DataToken dataToken, int remaimingBytesToProcess)
        {
            
            return remaimingBytesToProcess;
        }

        private static int HandleMessage(byte[] buffer, DataToken dataToken , int remaimingBytesToProcess)
        {
            int notCopyTypes = 0;
            if(dataToken.MessageDoneNum == 0)
            {
                dataToken.ByteArrayForMessage = new byte[dataToken.MessageLength];
            }
            var offset = dataToken.BufferSkipOffset;
            if (remaimingBytesToProcess + dataToken.MessageDoneNum > dataToken.MessageLength)
            {
                int needCopyBytes = dataToken.MessageLength - dataToken.MessageDoneNum;//需要复制的byte数
                notCopyTypes = remaimingBytesToProcess - needCopyBytes;//剩余的byte数
                Buffer.BlockCopy(buffer, offset, dataToken.ByteArrayForMessage, dataToken.MessageDoneNum, needCopyBytes);
                dataToken.MessageDoneNum = dataToken.MessageLength;
                dataToken.BufferSkipOffset += needCopyBytes;
            }
            else
            {
                Buffer.BlockCopy(buffer, offset, dataToken.ByteArrayForMessage, dataToken.MessageDoneNum , remaimingBytesToProcess);
                dataToken.MessageDoneNum += remaimingBytesToProcess;
            }
            return notCopyTypes;
        }
    }
}
