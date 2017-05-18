using System;
using UnityEngine;

namespace N2.Network
{
    class UnityLogger : Dev.ILogger
    {
        public bool Enabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Log(string format, params object[] p)
        {
            if(p!=null && p.Length>0)
                Debug.LogFormat(format, p);
            else 
                Debug.Log(format);
        }

        public void LogWarning(string format, params object[] p)
        {
            if (p != null && p.Length > 0)
                Debug.LogWarningFormat(format, p);
            else
                Debug.LogWarning(format);
        }

        public void LogError(string format, params object[] p)
        {
            if (p != null && p.Length > 0)
                Debug.LogErrorFormat(format, p);
            else
                Debug.LogError(format);
        }

        public void LogException(Exception e)
        {
            Debug.LogException(e);
        }

        public void Assert(bool condition, string msg, params object[] p)
        {
            Debug.AssertFormat(condition, msg, p);
        }
    }
}
