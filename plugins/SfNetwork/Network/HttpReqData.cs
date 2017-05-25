using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SF.Network.Socket
{
    /// <summary>
    /// System.WebRequest를 쉽게 재활용해서 사용하기 위해서 wraping해 놓은 객체
    /// </summary>
    public struct HttpReqData : IDisposable
    {
        public static HttpReqData Empty = new HttpReqData() { WebReq = null };

        internal HttpReqData(HttpWebRequest req, bool usePool)
        {
            WebReq = req;
            IsPooling = usePool;
        }

        /// <summary>
        /// 파라메터 데이터를 HTTP body에 저장합니다.
        /// </summary>
        /// <param name="bytes">저장 할 데이터</param>
        /// <param name="compressThreshold">지정한 용량을 넘어갈 경우 압축 처리, 만약 음수 값이면 압축 무시 </param>
        public void WriteData(byte[] bytes, HttpContentType contentType, uint compressThreshold = 4096)
        {
            if (WebReq == null)
                return;

            WebReq.Method = "POST";
            WebReq.ContentLength = bytes.Length;

            if (bytes.Length > compressThreshold)
            {
                WebReq.SendChunked = true;
                WebReq.TransferEncoding = "gzip";
            }
            else
            {
                WebReq.SendChunked = false;
                WebReq.TransferEncoding = null;
            }

            switch (contentType)
            {
                case HttpContentType.Json:
                    WebReq.ContentType = "Application/json";
                    break;

                case HttpContentType.Protobuf:
                    WebReq.ContentType = "n2pb";
                    //WebReq.Headers.Set("Encrypt", "pb");
                    break;

                default:
                    WebReq.ContentType = "meta";
                    break;
            }

            // 파라메터를 Stream으로 변환합니다.
            Stream reqStream = null;
            try
            {
                using (reqStream = WebReq.GetRequestStream())
                {
                    //512사이즈로 나누어 Stream 전송합니다.
                    const int WRITE_SIZE = 512;
                    int offset = 0;
                    int size = (int)bytes.Length;
                    for (;;)
                    {
                        var count = offset + WRITE_SIZE > size ? size - offset : WRITE_SIZE;
                        reqStream.Write(bytes, offset, count);
                        if (count == (size - offset)) break;
                        offset += count;
                    }
                }
            }
            finally
            {
                if (reqStream != null)
                    reqStream.Close();
            }
        }

        /// <summary>
        /// 서버에서 보내온 Response 정보를 가져옵니다.
        /// </summary>
        public HttpWebResponse GetResponse()
        {
            // Get 방식일때는 GetResponse() 호출 시점에 서버에 Request 패킷 전송
            // Post 방식일때는 Stream에 데이터가 다 기록될때 전송 됨

            return WebReq != null ? (HttpWebResponse)WebReq.GetResponse() : null;
        }

        /// <summary>
        /// 서버로 부터 응답을 받았는지 여부
        /// </summary>
        public bool HaveResponse
        {
            get { return WebReq != null && WebReq.HaveResponse; }
        }

        /// <summary>
        /// WebRequest 인스턴스 유무
        /// </summary>
        public bool IsValidInatnce
        {
            //get { return WebReq == null || !WebReq.HaveResponse; }
            get { return WebReq != null; }
        }

        /// <summary>
        /// Pool을 통해 인스턴스 관리를 하는지 여부
        /// </summary>
        public bool IsPooling { get; internal set; }

        public void Dispose()
        {
            //if(WebReq!=null)
            //    WebReq.ContentLength = 0;

            HttpReqDataFactory.ReleaseSocket(this);
        }

        internal HttpWebRequest WebReq { get; private set; }
        
        public static implicit operator HttpWebRequest(HttpReqData socket)
        {
            return socket.WebReq;
        }

        /// <summary>
        /// 풀애 았눈 소켓을 전부 비웁니다.
        /// </summary>
        public static void ReleaseSocketPool()
        {
            HttpReqPool.ReleasePool();
        }
    }
}
