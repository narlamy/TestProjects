using System;
using System.Collections.Generic;

namespace SF.Network
{
    /// <summary>
    /// 접속할 서버의 정보를 설정합니다.
    /// 접속 URL, Port 및 서버와 통신이 이뤄질 프로토콜 정의를 합니다.
    /// </summary>
    public abstract class Connection
    {
        const int DEFUALT_TIMEOUT_MSEC = 7 * 1000;  // 기본값 : 7초
        public System.Uri Uri { get; protected set; }

        private bool mEncrypted = false;
        public bool Encrypted { get { return mEncrypted; } protected set { mEncrypted = value; } }

        private int mTimeout = DEFUALT_TIMEOUT_MSEC;
        public int Timeout { get { return mTimeout; } set { mTimeout = value; } }

        public string URL { get { return Uri.Scheme + "://" + Uri.Host; } }
        public int Port { get { return Uri.Port; } }

        /// <summary>
        /// 타임 아웃 시간을 설정합니다.
        /// 편의상 Seconds 단위로 입력받습니다.
        /// </summary>
        public Connection SetTimeout(float secs)
        {
            Timeout = (int)(secs * 1000.0f);
            return this;
        }

        public Connection UseEncryption(bool encryption)
        {
            Encrypted = encryption;
            return this;
        }

        public Connection()
        {
        }

        public Connection(Uri uri)
        {
            this.Uri = uri;
        }

        public ISocket Socket { get; protected set; }

        public Connection SetListner(OnListen onListener)
        {
            if(Socket is IDuplexSocket)
            {
                ((IDuplexSocket)Socket).SetListner(onListener);
            }
            return this;
        }

        public Connection RemoveListner()
        {
            if (Socket is IDuplexSocket)
            {
                ((IDuplexSocket)Socket).RemoveListner();
            }
            return this;
        }
    }

    /// <summary>
    /// 접속할 서버의 주소를 변경합니다.
    /// 패킷을 전송할떄 기본적으로 해당 주소를 통해 접속합니다.
    /// </summary>
    public struct ServerChanger : IDisposable
    {
        private Connection mOldConnection;
        private IConnectionChanger mChanger;

        public ServerChanger(Connection connection)
        {
            mChanger = Sender.Instance as IConnectionChanger;

            if (mChanger != null)
            {
                mOldConnection = mChanger.Connection;
                mChanger.Connection = connection;
            }
            else
            {
                mOldConnection = null;
            }
        }

        public void Dispose()
        {
            if (mChanger != null)
                mChanger.Connection = mOldConnection;

            mOldConnection = null;
            mChanger = null;
        }
    }
}
public static class ConnectionUtil
{
    /// <summary>
    /// 해당 정보를 가지고 서버 접속 주소를 변경합니다.
    /// Dispose 객체를 반환하므로 반환 된 객첵의 Dispose()를 호출하면 
    /// 이전 접속 주소로 복구됩니다.
    /// 
    /// using() {} 블록을 사용해서 호출하는 것을 권장합니다.
    /// </summary>
    public static IDisposable Change(this SF.Network.Connection connection)
    {
        return new SF.Network.ServerChanger(connection);
    }        
}
