using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace SF.Network
{
    internal static class PacketTable
    {
        static IPacketTable mInstance = null;

        static public IPacketTable Instance { get { return mInstance; } }
        static PacketTable()
        {
            mInstance = ModuleFactory.CreatePacketTable();
        }
    }
}
