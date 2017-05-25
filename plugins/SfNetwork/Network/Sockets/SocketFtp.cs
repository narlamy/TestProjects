using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SF.Network.Sockets
{
    /// <summary>
    /// Ftp 연결 소켓
    /// </summary>
    internal class SocketFtp : ISimplexSocket
    {
        public static SocketFtp DefaultSocket = new SocketFtp();

        public void Send(RequestPacket req, OnResponse onResponse)
        {

        }
    }
}
