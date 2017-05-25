using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LitJson;

namespace SF.Network.Packet
{
    /// <summary>
    /// 지정한 타입의 에러 코드에 접근할 수 있는 응답 패킷 클래스
    /// 패킷으로는 음수로 표현된 정수형 에러값이 날아오는데
    /// 지정한 타입 (ERROR_TYPE)으로 형변환해서 반환합니다.
    /// </summary>
    [System.Reflection.Obfuscation]
    public abstract class ResponsePacket<ERROR_TYPE> : ResponsePacket where ERROR_TYPE : ServerError, new()
    {
        /// <summary>
        /// 어떤 패킷에서 발생한 에러인지 쉽게 파악이 가능한 에러 객체를 반환합니다.
        /// </summary>
        [JsonIgnore]
        public ERROR_TYPE Error
        {
            get
            {
                try
                {
                    var error = new ERROR_TYPE()
                    {
                        ErrorCode = mErrorCode,
                        PacketUniqueNum = GetUniqueNum()
                    };
                    return error;
                }
                catch
                {
                    return new ERROR_TYPE() { ErrorCode = ServerError.None };
                }
            }
        }

        internal override ServerError GetError()
        {
            return Error;
        }
    }
}
