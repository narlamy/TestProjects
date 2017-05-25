using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SF.Network.Socket
{ 
    /// <summary>
    /// 웹 소켓 재활용을 위한 풀
    /// </summary>
    internal static class HttpReqPool
    {
        static Dictionary<string, Queue<HttpWebRequest>> mPool = new Dictionary<string, Queue<HttpWebRequest>>();

        /// <summary>
        /// 더 이상 사용하지 않는 소켓을 풀에 넣어둡니다.
        /// </summary>
        public static void RestoreSocket(HttpWebRequest webRequest)
        {
            var path = webRequest.RequestUri.AbsolutePath;
            Queue<HttpWebRequest> queue;

            if (!mPool.TryGetValue(path, out queue))
            {
                queue = new Queue<HttpWebRequest>();
                mPool.Add(path, queue);
            }
            queue.Enqueue(webRequest);
        }

        /// <summary>
        /// 풀에서 보관중인 소켓을 꺼내옵니다.
        /// </summary>
        public static HttpWebRequest PickUpSocket(Uri uri)
        {
            var path = uri.AbsolutePath;
            Queue<HttpWebRequest> queue;

            if (!mPool.TryGetValue(path, out queue))
            {
                queue = new Queue<HttpWebRequest>();
                mPool.Add(path, queue);
            }

            if (queue.Count == 0)
            {
                var webReqeust = (HttpWebRequest)WebRequest.Create(uri);
                return webReqeust;
            }
            else
            {
                var webRequest = queue.Dequeue();
                return webRequest;
            }
        }

        /// <summary>
        /// Pool을 모두 날립니다.
        /// </summary>
        public static void ReleasePool()
        {
            mPool.Clear();
        }
    }
}
