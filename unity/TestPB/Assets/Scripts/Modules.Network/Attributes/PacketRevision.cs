using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N2.Network
{
    /// <summary>
    /// 패킷 리비전
    /// </summary>
    public class PacketRevisionAttribute : Attribute
    {
        public PacketRevisionAttribute(int revision)
        {
            Revision = revision;
        }

        public int Revision { get; private set; }
    }
}
