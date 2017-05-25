using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SF.Network.Sockets
{
    internal class SocketDummy : ISimplexSocket
    {
        public static SocketDummy DefaultSocket = new SocketDummy();

        public void Send(RequestPacket req, OnResponse onResponse)
        {
        }
    }
}
