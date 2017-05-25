using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SF.Network
{
    public enum ConnectionType
    {
        Http,
        Tcp,
        Ftp,
    }

    /// <summary>
    /// 커넥션 생성
    /// </summary>
    public static class ConnectionFactory
    {
        /// <summary>
        /// 프로토콜에 맞는 커넥션 생성
        /// </summary>
        /// <param name="type"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static Connection Create(ConnectionType type, Uri uri)
        {
            switch(type)
            {
                case ConnectionType.Http:
                    return new Connections.ConnectionHttp(uri);

                case ConnectionType.Tcp:
                    return new Connections.ConnectionTcp(uri);

                default:
                    return Connections.ConnectionDummy.DEFAULT;
            }
        }
    }
}
