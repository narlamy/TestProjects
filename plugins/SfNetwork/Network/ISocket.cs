using System;
namespace SF.Network
{
    /// <summary>
    /// 서버와 패킷을 통신하는 추상 객체
    /// </summary>
    public interface ISocket
    {        
    }

    /// <summary>
    /// 
    /// </summary>
	public interface ISimplexSocket : ISocket
	{
		void Send(RequestPacket req, OnResponse onResponse);
	}

	public delegate void OnListen(RequestPacket req);

    /// <summary>
    /// connected sokcet인 경우는 서버에서 보내오는 패킷에 응답 필요 (TCP/IP)
    /// </summary>
	public interface IDuplexSocket : ISimplexSocket
	{
		void SetListner(OnListen onListen);

        /// <summary>
        /// 
        /// </summary>
		void RemoveListner();

        /// <summary>
        /// 서버 연결
        /// </summary>
        void Connect();

        /// <summary>
        /// 수신 받을 준비
        /// </summary>
        void Listen();

        /// <summary>
        /// 서버 연결 해제
        /// </summary>
        void Disconnect();
	}
}
