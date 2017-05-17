
using System;

namespace N2.Network
{
    /// <summary>
    /// 네트워크 모듈에서 사용하는 로그, 콜백 등의 객체 생성
    /// </summary>
    public class ProxyFactory
    {
        static IProxyFactory mInstance = null;

        internal static IProxyFactory Instance 
        { 
            get
            {
                if (mInstance == null)
                {
                    switch (PlatformSelector.Platform)
                    {
                    case Platform.General:
                        mInstance = new GeneralProxyFactory();
                        break;

                    case Platform.Unity:
                        mInstance = new UnityProxyFactory();
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
