using System;
namespace N2.Network
{
	public interface ISocket : IListener
	{
		void Request(RequestPacket req);
	}

	public interface ISender
	{
		void Send(RequestPacket req);
	}

	public delegate void OnListen(RequestPacket req);

	public interface IListener
	{
		void Add(OnListen onListen);
		void Remove(OnListen onListen);
	}
}
