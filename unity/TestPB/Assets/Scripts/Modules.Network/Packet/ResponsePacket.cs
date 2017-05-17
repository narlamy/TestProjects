﻿using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace N2.Network
{
    /// <summary>
    /// 추상 응답 패킷
    /// RequestPacket으로 보낸 후 서버로부터 받음
    /// </summary>
    [System.Reflection.Obfuscation]
    public abstract class ResponsePacket
    {
        public string PacketName { get { return GetType().Name; } }

        private RequestPacket mRequestPacket = null;

        internal void SetRequestPacket(RequestPacket req)
        {
            mRequestPacket = req;
        }

        public RequestPacket GetRequestPacket()
        {
            return mRequestPacket;
        }

        [JsonProperty("ErrorCode")]
        protected int mErrorCode = 0;    // 서버로부터 특별한 에러코드를 받지 않으면 성공인 0으로 처리

        public int GetErrorCode()
        {
            return mErrorCode;
        }

        /// <summary>
        /// 요청에 대한 성공 여부 반환
        /// </summary>
        [JsonIgnore]
        public virtual bool IsSuccess 
        { 
            get { return mErrorCode == 0; } 
        }

        [JsonProperty("Message", NullValueHandling = NullValueHandling.Ignore)]
        private string mMessage = "";

        /// <summary>
        /// 에러 메세지 (옵션)
        /// </summary>
        [JsonIgnore]
        public string ErrorMessage { get { return mMessage; } }

        /// <summary>
        /// 패킷 고유 번호
        /// </summary>
        protected int GetUniqueNum()
        {
            //return PacketTable.Instance.GetUniqueNum(GetType());
            return 0;
        }
        
        /// <summary>
        /// 에러 코드를 변경합니다.
        /// </summary>
        internal void SetErrorCode(int errCode, string errMsg=null)
        {
            mErrorCode = errCode;

            if (errMsg != null)
                mMessage = errMsg;
        }

        internal abstract ServerError GetError();
        
        /// <summary>
        /// RequestPacket.Send() 호출시 연결된 콜백을 반환합니다.
        /// </summary>
        /// <returns></returns>
        public OnResponse GetResponseCallback()
        {
            return (mRequestPacket != null) ? mRequestPacket.ResponseCallback : null;
        }
    }

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
