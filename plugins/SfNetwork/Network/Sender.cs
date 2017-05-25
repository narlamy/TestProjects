namespace SF.Network
{
    /// <summary>
    /// 패킷 전송
    /// </summary>
    internal interface ISender
    {
        /// <summary>
        /// 서버의 접속 주소 및 소켓 프로토콜 등의 정의
        /// </summary>
        Connection Connection { get; }

        /// <summary>
        /// 서버에 요청 후 서버의 응답을 기다리는 패킷의 수
        /// </summary>        
        /// <value> 기본값=1</value>
        int ConcurrentCount { get; set; }

        /// <summary>
        /// RequestPacket을 서버에 전송(요청) 합니다.
        /// 서버로부터 응답된 결과는 onReponse로 전달됩니다.
        /// </summary>
        void Send(RequestPacket req, OnResponse onResponse);
    }

    /// <summary>
    /// 커넥션 접근 및 교체
    /// </summary>
    internal interface IConnectionChanger
    {
        Connection Connection { get; set; }
    }

    /// <summary>
    /// 요청한 패킷을 순차적으로 서버에 전송해주는 싱글톤 객체입니다.
    /// HTTP,TCP등의 프로토콜과 접속 주소가 설정 된 Connection 인스턴스를 참조해서
    /// 서버에 패킷을 전달합니다.
    /// 
    /// RequestPacket 클래스를 통해 요청을 하게 되므로 직접 접근할 필요는 없습니다.
    /// </summary>
    internal static class Sender
    {
        private static ISender mInstance = null;
        public static ISender Instance
        {
            get { return mInstance; }
        }

        static Sender()
        {
            //var connection = new HttpJsonConnection() { Uri = new System.Uri("http://192.168.0.1:17761") };
            mInstance = new ImpSender(Connections.ConnectionDummy.DEFAULT);
        }
    }
}
