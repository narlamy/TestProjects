using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace N2.Network
{
    internal static class PacketTable
    {
        static IPacketTable mInstance = null;

        static public IPacketTable Instance
        {
            get
            {
                if (mInstance == null)
                {
                    switch (PlatformSelector.Platform)
                    {
                    case Platform.General:
                        mInstance = new GeneralPacketTable();
                        break;

                    case Platform.Unity:
                        mInstance = new UnityPacketTable();
                        break;
                    }
                }
                return mInstance;
            }
        }
    }
}
