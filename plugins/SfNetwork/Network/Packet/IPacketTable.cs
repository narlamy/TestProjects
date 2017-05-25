using System;

namespace SF.Network
{
	internal interface IPacketTable
	{
		int GetUniqueNum(Type type);

		string FindPacketName(int uniqueNum);

		int Count { get; }
	}
}
