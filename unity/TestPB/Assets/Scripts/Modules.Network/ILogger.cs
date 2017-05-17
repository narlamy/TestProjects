using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N2.Network
{
    public interface INetLogger
    {
        void Log(string format, params object[] p);
        void LogWarning(string format, params object[] p);
        void LogError(string format, params object[] p);
    }
}
