using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SF.Network
{
    /// <summary>
    /// 접속 할 서버의 접속 URL을 미리 등록해두고 등록 된 키값을 이용해서
    /// 손쉽게 접속 서버를 변경할 수 있습니다.
    /// </summary>
    public static class ServerConnections
    {
        public static Dictionary<string, Connection> mConnections = new Dictionary<string, Connection>();


        /// <summary>
        /// 해당 키로 서버 접속 주소를 등록합니다.
        /// URL 뿐 아니라 패킷 프로토콜도 함께 설정되어 있습니다.
        /// </summary>
        public static void AddConnection(string key, Connection connection)
        {
            mConnections[key] = connection;
        }

        /// <summary>
        /// 
        /// </summary>
        public static Connection GetConnection(string key)
        {
            Connection connection;
            return mConnections.TryGetValue(key, out connection) ? connection : null;
        }

        /// <summary>
        /// 접속할 서버를 변경합니다.
        /// </summary>
        public static ServerChanger Change(string KEY)
        {
            var connection = ServerConnections.GetConnection(KEY);
            if (connection != null)
            {
                return new ServerChanger(connection);
            }
            else
            {
                return new ServerChanger();
            }
        }

        public static ServerChanger AddAndChange(string key, Connection connection)
        {
            AddConnection(key, connection);
            return new ServerChanger(connection);

        }
    }
}
