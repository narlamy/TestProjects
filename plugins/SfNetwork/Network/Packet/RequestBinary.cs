using System;
using System.IO;
using LitJson;

namespace SF.Network
{
    /// <summary>
    /// 전송할 데이터를 직접 구현하지 않고 Binary 데이터를 입력받아 처리하는 요청 패킷
    /// </summary>
    public sealed class RequestBinary : RequestPacket
    {
        /// <summary>
        /// 당장 지원은 하지 않지만, 파일을 첨부해서 HTTP 패킷을 보낼 수 있도록 합시다.
        /// 테이블 파일을 보낼때 유리할 것 같습니다.
        /// </summary>
        public RequestBinary(string reqName, byte[] bytes)
        {
            mReqName = reqName;
            Bytes = bytes;
        }

        public RequestBinary(string reqName, Func<byte[]> serializer)
        {
            mReqName = reqName;
            Serializer = serializer;
        }

        string mReqName;
        byte[] mBytes = null;

        /// <summary>
        /// 첨부된 바이너리 데이터
        /// </summary>
        [JsonIgnore]
        public byte[] Bytes
        {
            get
            {
                if (mBytes == null && Serializer != null)
                {
                    mBytes = Serializer();
                }
                return mBytes;
            }
            private set { mBytes = value; }
        }

        public override string PacketName { get { return mReqName; } }

        [JsonIgnore]
        public override Type ResponseType
        {
            get { return typeof(ResponseBinary); }
        }

        Func<byte[]> Serializer = null;

        public RequestBinary SetSerializer(Func<byte[]> serializer)
        {
            Serializer = serializer;
            return this;
        }

        internal override byte[] GetBytes()
        {
            return Bytes;
        }

        public override void Dispose()
        {
            Bytes = null;
        }

        public ResponseBinary Response
        {
            get { return mResponsePacket as ResponseBinary; }
        }

        internal override bool HasParameter
        {
            get { return Bytes != null && Bytes.Length > 0; }
        }

        internal override ResponsePacket ParseResponse(Stream stream, long streamLength, string contentType)
        {
            if (stream != null)
            {
                const int CHUNK_SIZE = 512;
                byte[] buffer = new byte[streamLength];
                int offset = 0;

                try
                {
                    while (offset < streamLength)
                    {
                        var nextOffset = offset + CHUNK_SIZE;
                        var size = (nextOffset < streamLength) ? CHUNK_SIZE : (int)(streamLength % CHUNK_SIZE);

                        stream.Read(buffer, offset, size);
                        offset = nextOffset;
                    }
                }
                catch(Exception e)
                {
                    Dev.Logger.LogException(e);
                }

                return new ResponseBinary(this, buffer);
            }
            else
            {
                return this.CreateResponse(ServerError.StreamError);
            }
        }
    }
}
