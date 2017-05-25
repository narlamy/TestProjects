using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using LitJson;

namespace SF.Network
{
    // <summary>
    /// Body에 JSON
    /// </summary>
    public abstract class RequestJson : RequestPacket
    {
        string mSerializedParams = "";

        /// <summary>
        /// 패킷 전송 요청시에 미리 시리얼라이징을 처리합니다.
        /// 프리징 현상이 생길 수도 있는데 음... 
        /// 매인 쓰레드 문제가 발생할 수도 있어서 백그라운드에서 처리해도 될지는 봐야겠습니다.
        /// </summary>
        protected string SerializeParameters()
        {
            lock (mSerializedParams)
            {
                if (string.IsNullOrEmpty(mSerializedParams))
                {
                    try
                    {
                        //mSerializedParams = JsonConvert.SerializeObject(this, Formatting.None);
                        mSerializedParams = LitJson.JsonMapper.ToJson(this);
                    }
                    catch (System.Exception e)
                    {
                        mSerializedParams = string.Format("{\"Exception\" = \"{0}\"}", e.ToString());
                    }
                }
            }
            return mSerializedParams;
        }

        public override void Dispose()
        {
            mSerializedParams = null;
        }

        internal override bool HasParameter
        {
            get { return !string.IsNullOrEmpty(SerializeParameters()); }
        }

        internal override byte[] GetBytes()
        {
#if ENABLE_ENCRYPT
                //암호화 
                if (useEncrypt)
                {
                    const string BIN = "FCFE";
                    var encyptParam = PakêtêŞîfrekirinn.Ｅｎcｒｉｐt(packetParam, reqPacket.HashKey);

                    // JSON 형식 사용 여부에 따라 암호화 된 이후 조합 방식이 달라집니다.
                    if (reqPacket.IsJsonParam)
                    {
                        packetParam = "{ \"" + BIN + "\" : \"" + encyptParam + "\" }";
                    }
                    else
                    {
                        packetParam = BIN + "=" + encyptParam;
                    }
                }
#endif
            return System.Text.Encoding.UTF8.GetBytes(SerializeParameters());
        }

        protected override void _BeforeRequest()
        {
            // [2017-05-24] narlamy
            // 일반적으로 패킷 요청은 매인 쓰래드에서 일어나기 때문에
            // 프로토콜 시리얼라이징을 매인 쓰래드에서 처리하기 위해 사용합니다.
            // GameObject 등 유니티 객체를 다른 쓰래드에서 시리얼라이징시 접근하면
            // 에러가 발생합니다.

            SerializeParameters();
        }

        /// <summary>
        /// JSON 형식의 응답 데이터를 파싱해서 ResponsePacket 인스턴스를 생성합니다.
        /// </summary>
        internal override ResponsePacket ParseResponse(Stream responseStream, long streamLength, string contentType)
        {
            // RequestPakcet.ResponseType에 연결 된 클래스를 자동 생성해서 반환합니다.            

            if (responseStream != null)
            {
                using (TextReader reader = new StreamReader(responseStream))
                {
                    // PacketFactory에 의해 컨텐츠 내용에 따라 적절히 파싱한 후에
                    // Deserialize로 ResponsePacket 인스턴스를 생성합니다.

                    string content = reader.ReadToEnd();
                    return this.CreateResponse(content);
                }
            }
            else
            {
                return this.CreateResponse(ServerError.StreamError);
            }
        }        
    }

    /// <summary>
    /// 패킷 전송시 사용하는 데이터 포맷
    /// </summary>
    public enum HttpContentType
    {
        Json,
        Protobuf
    };

    /// <summary>
    /// RESPONSE_TYPE으로 ResponseType을 반환하도록 구현된 요청 패킷
    /// </summary>
    [System.Reflection.Obfuscation]
    public abstract class RequestPacket<RESPONSE_TYPE> : RequestJson
        where RESPONSE_TYPE : ResponsePacket
    {
        protected RequestPacket()
        {

        }

        protected RequestPacket(HttpContentType paramMode)
        {
            DataFormat = paramMode;
        }

        HttpContentType DataFormat = HttpContentType.Json;

        [JsonIgnore]
        public sealed override System.Type ResponseType
        {
            get { return typeof(RESPONSE_TYPE); }
        }

        /// <summary>
        /// Send(onResponse) 호출 후에 콜백 인자로 받은 ResponePacket을
        /// 적합한 타입으로 바로 캐스팅합니다.
        /// </summary>
        public RESPONSE_TYPE CastResponse(ResponsePacket resPacket)
        {
            return resPacket as RESPONSE_TYPE;
        }

        /// <summary>
        /// 콜백을 사용하지 않은 Send()를 사용한 경우에 패킷 수신이 완료되면
        /// RequestPacket에 세팅된 Response 인스턴스를 반환합니다.
        /// </summary>
        [JsonIgnore]
        public RESPONSE_TYPE Response
        {
            get
            {
                return mResponsePacket as RESPONSE_TYPE;
            }
        }
    }
}
