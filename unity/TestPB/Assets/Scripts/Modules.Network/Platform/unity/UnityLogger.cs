using UnityEngine;

namespace N2.Network
{
    class UnityLogger : INetLogger
    {
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
    }
}
