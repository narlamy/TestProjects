using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SF.Network.Sockets
{
    /// <summary>
    /// JSON 형식의 HTTP 패킷으로 통신하는 소켓
    /// </summary>
    internal class SocketHttp : ISimplexSocket
    {
        public static SocketHttp DefaultSocket = new SocketHttp();

        public void Send(RequestPacket req, OnResponse onResponse)
        {

        }
    }
}
