using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N2.Network
{
    public enum Platform
    {
        General,
        Unity
    }
    
    public static class PlatformSelector
    {
        static private Platform mPlatform = Platform.Unity;
        static public Platform Platform
        {
            get { return mPlatform; }
            set 
            {
                if (mPlatform != value)
                {
                    ProxyFactory.Reset();
                    NetLogger.Reset();
                }
                mPlatform = value; 
            }
        }
    }
}
