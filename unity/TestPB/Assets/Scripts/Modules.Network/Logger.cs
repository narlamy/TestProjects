using System;

namespace N2.Network
{
    internal static class NetLogger
    {
        static INetLogger mInstance = null;
        
        public static INetLogger Instance 
        {
            get
            {
                if (mInstance == null)
                {
                    switch (PlatformSelector.Platform)
                    {
                    case Platform.General:
                        mInstance = new GeneralLogger();
                        break;

                    case Platform.Unity:
                        mInstance = new UnityLogger();
                        break;
                    }
                }
                return mInstance;
            }
        }

        internal static void Reset() 
        { 
            mInstance = null; 
        }
    }
}
