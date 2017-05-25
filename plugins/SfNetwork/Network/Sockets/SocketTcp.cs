using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SF.Network.Sockets
{
    /// <summary>
    /// TCP 연결 소켓
    /// </summary>
    internal class SocketTcp : IDuplexSocket
    {
        public static SocketTcp DefaultSocket = new SocketTcp();

        public void Connect()
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Listen()
        {
            throw new NotImplementedException();
        }

        public void RemoveListner()
        {
            throw new NotImplementedException();
        }

        public void Send(RequestPacket req, OnResponse onResponse)
        {

        }

        public void SetListner(OnListen onListen)
        {
            throw new NotImplementedException();
        }
    }
}
