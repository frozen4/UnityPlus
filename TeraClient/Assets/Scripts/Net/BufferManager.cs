using System.Collections.Generic;
using System.Net.Sockets;

namespace Common.Net
{
#if UES_4_SERVER
    //每个节点（service/customer）都有一个
    //防止内存碎片，全局只有一个，接受、发送共用一个大内存
    //服务器与客户端、服务器与服务器之间通信的socket缓存都在这里分配
    //所以Buffer总大小为：MAX_CLIENT_CONNECTION + MAX_SERVERNODE_CONNECTION
    public class BufferManager
    {
        //总内存大小
        private int _capability;
        //每块内存的大小，每个socketAsyncEventArgs可用的最大buffer
        private int _buffsize_for_each_block;
        public int BUFFER_SIZE_FOR_EACH_BLOCK { get; private set; }
        //空闲索引栈
        private Stack<int> _freeIndexPool;
        //当前的索引
        private int _currentIndex;
        //总内存
        private byte[] _bufferBlock;
        public BufferManager(int capablity,int  buffsize_for_each_block)
        {
            _capability = capablity;
            _buffsize_for_each_block = buffsize_for_each_block;
            BUFFER_SIZE_FOR_EACH_BLOCK = buffsize_for_each_block;
            _currentIndex = 0;
            _freeIndexPool = new Stack<int>();
        }

        public void InitBuffer()
        {
            _bufferBlock = new byte[_capability];
        }

        //为指定的SocketAsyncEventArgs分配缓存
        public bool SetBuffer(SocketAsyncEventArgs saea)
        {
            if(_freeIndexPool.Count > 0 )
            {
                saea.SetBuffer( _bufferBlock, _freeIndexPool.Pop(), _buffsize_for_each_block);
            }
            else
            {
                //如果当前可分配内存已用尽
                if((_capability - _buffsize_for_each_block) < _currentIndex)
                {
					HobaDebuger.Log("SetBuffer no enough buffer.");
                    return false;
                }
                saea.SetBuffer( _bufferBlock, _currentIndex, _buffsize_for_each_block);
                _currentIndex += _buffsize_for_each_block;
            }
            return true;
        }
        //一般情况下，SocketAsyncEventArgs池中每个SAEA都会对应一个buffer，不需要释放
        //除非想要销毁SAEA对象
        //（这种情况一般发生在程序结束，Netclient 主动关闭的时候，server的SocketAsyncEventArgs是重用的，所以不用调）
        public void FreeBuffer(SocketAsyncEventArgs saea)
        {
            _freeIndexPool.Push(saea.Offset);
            saea.SetBuffer(null, 0, 0);
        }
        public void FreeBuffer(int offset)
        {
            _freeIndexPool.Push(offset);
        }

    }
#endif
}
