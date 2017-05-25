using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SF.Network.Socket
{
    /// <summary>
    /// HTTP 패킷을 서버에 요청할 수 있는 객체를 반환합니다.
    /// 옵션에 따라 바로 생성하거나 풀링된 객체를 반환합니다.
    /// </summary>
    internal static class HttpReqDataFactory
    {
        // [2016-10-22] narlamy
        // HttpWebRequest 인스턴스를 재활용 (pool,cahcing) 하려고
        // SocketPool을 만들어보긴 헀지만 HttpWebRequest 객체 자체가
        // 생성 이후 URL을 변경할 수 없습니다. 따라서 재활용이 불가능합니다.
        // 일단 pool은 사용하지 않도록 하겠습니다.

        static bool mEnablePool = false;

        /// <summary>
        /// Pooling 요청시 SocketPool 사용해서 Socket 인스턴스를 생성할지 여부
        /// </summary>
        public static bool EnablePool
        {
            get { return mEnablePool; }
            set { }
            //set { mEnablePool = value; }  // [2016-10-22] narlamy : 사용 안 함
        }
        
        public static HttpReqData Create(string uriString, int timeoutSecs, CookieContainer cookies, bool pooling = true)
        {
            Uri uri = new Uri(uriString);
            return Create(uri, timeoutSecs, cookies, pooling);
        }

        /// <summary>
        /// 소켓 생성
        /// </summary>
        public static HttpReqData Create(Uri uri, int timeoutSecs, CookieContainer cookies, bool isPooling = true)
        {
            HttpReqData socket;
            HttpWebRequest webRequest;

            if (EnablePool && isPooling)
            {
                socket = new HttpReqData(HttpReqPool.PickUpSocket(uri), true);
                webRequest = socket;
                webRequest.KeepAlive = false;
                webRequest.Headers.Clear();

                // [2016-10-22] narlamy
                // 이미 생성 된 HttpWebRequst의 URL을 변경할 수 없기 때문에 풀에 
                // 넣어 두고 재사용하는것에 한계가 있습니다.
                // BasePath만 고정시키고 query는 계속해서 변경하고 싶은데 그게 안 되는거죠.
            }
            else
            {
                webRequest = (HttpWebRequest)WebRequest.Create(uri);
                webRequest.KeepAlive = false;
                socket = new HttpReqData(webRequest, false);
            }

            webRequest.CookieContainer = cookies;

            webRequest.ContentType = "Application/json"; //isJsonParam ? "Application/json" : "application/x-www-form-urlencoded";
            webRequest.Method = "GET";
            webRequest.ContentLength = 0;
            webRequest.Timeout = timeoutSecs;
            webRequest.Proxy = null; //프로시 서버 검색 시도를 막습니다.

            return socket;
        }

        /// <summary>
        /// 소켓 전송 후 메모리 해제 (반환)
        /// </summary>
        public static void ReleaseSocket(HttpReqData socket)
        {
            if (socket.WebReq == null)
                return;

            if(socket.IsPooling)
            {
                HttpReqPool.RestoreSocket(socket.WebReq);
            }
            else
            {
                socket.WebReq.Abort();
            }
        }
    }    
}
