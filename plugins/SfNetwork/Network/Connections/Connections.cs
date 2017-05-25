using System;

namespace SF.Network.Connections
{
    internal class ConnectionDummy : Connection
    {
        static internal ConnectionDummy DEFAULT = new ConnectionDummy();

        public ConnectionDummy()
        {
            Uri = new Uri("http://192.168.0.1:17761");
            Socket = Sockets.SocketDummy.DefaultSocket;
        }
    }

    public class ConnectionHttp : Connection
    {
        public ConnectionHttp(Uri uri) : base(uri)
        {
            Socket = Sockets.SocketHttp.DefaultSocket;
        }
    }

    public class ConnectionTcp : Connection
    {
        public ConnectionTcp(Uri uri) : base(uri)
        {
            Socket = Sockets.SocketTcp.DefaultSocket;
        }
    }
}
