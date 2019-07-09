using System;
using System.Net.Sockets;

namespace Common.Net
{
    public class ExSocket
    {
        private Socket _socket;

        public bool Connected
        {
            get
            {
                return _socket != null ? _socket.Connected : false;
            }
        }

        public ExSocket(AddressFamily family, SocketType type, ProtocolType proto)
        {
            _socket = new Socket(family, type, proto);
        }

        public bool ConnectAsync(SocketAsyncEventArgs e)
        {
            if (_socket == null) return true;
            try
            {
                return _socket.ConnectAsync(e);
            }
            catch (Exception ex)
            {
                HobaDebuger.Log(HobaText.Format("Socket.ConnectAsync Exception: {0}", ex.Message));
            }

            return true;
        }

        public bool ReceiveAsync(SocketAsyncEventArgs e)
        {
            if (_socket == null) return true;

            try
            {
                return _socket.ReceiveAsync(e);
            }
            catch (Exception ex)
            {
                HobaDebuger.Log(HobaText.Format("Socket.ReceiveAsync Exception: {0}", ex.Message));
            }

            return true;
        }

        public int Send(byte[] buf, int size)
        {
            if(_socket == null) return -1;

            return  _socket.Send(buf, size, SocketFlags.None);
        }

        public void Close()
        {
            try
            {
                if (_socket != null)
                {
                    if(_socket.Connected)
                        _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();
                    HobaDebuger.Log(HobaText.Format("Socket closed successfully"));
                    //_socket.Dispose();
                }
            }
            catch(Exception ex)
            {
                HobaDebuger.Log(HobaText.Format("ExSocket close error: {0}", ex.Message));
            }
        }
    }
}
