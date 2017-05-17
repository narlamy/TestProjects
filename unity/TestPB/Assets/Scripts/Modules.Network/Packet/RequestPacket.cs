using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace N2.Network
{
    public delegate void OnResponse(ResponsePacket resonse);

    /// <summary>
    /// 추상 요청 패킷
    /// 서버에 무엇인가를 요청하거나 전달 함
    /// </summary>
    [System.Reflection.Obfuscation]
    public abstract class RequestPacket
    {
        public RequestPacket()
        {
        }

        /// <summary>
        /// 패킷 전송 시간
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(IsoDateTimeConverter))]
        internal DateTime TimeStamp { get; set; }

        public string PacketName { get { return GetType().Name; } }

        [JsonIgnore]
        public abstract System.Type ResponseType { get; }

        [JsonIgnore]
        public abstract string Parameters { get; }

        [JsonIgnore]
        internal bool? IsIgnoreConnecting = null;

        [JsonIgnore]
        internal bool? IsIgnoreRetry = null;

        /// <summary>
        /// HTTPS 사용 여부
        /// </summary>
        [JsonIgnore]
        public bool IsSecurity
        {
            get { return mIsSecurity; }
            protected set { mIsSecurity = value; }
        }

        bool mIsSecurity = false;

        /// <summary>
        /// 패킷을 보냅니다.
        /// </summary>
        public virtual void Send(OnResponse onResponse)
        {
            if(onResponse!=null)
                ResponseCallback = onResponse;

            N2.Network.Sender.Instance.Send(this, onResponse);
        }

#if USE_WEAK_REF
        private WeakReference<OnResponse> mOnResponseCallback = null;
        
        /// <summary>
        /// Send() 시 입력한 onResponse 콜백 반환
        /// </summary>
        internal OnResponse ResponseCallback
        {
            get
            {
                return mOnResponseCallback.IsAlive() ? mOnResponseCallback.Instance : null;
            }
            private set
            {
                mOnResponseCallback = value;
            }
        }
#else
        private OnResponse mOnResponseCallback = null;

        /// <summary>
        /// Send() 시 입력한 onResponse 콜백 반환
        /// </summary>
        [JsonIgnore]
        internal OnResponse ResponseCallback
        {
            get { return mOnResponseCallback; }
            private set { mOnResponseCallback = value; }
        }
#endif
        /// <summary>
        /// 서버에 요청을 보낸 상태인지 아닌지 알려줍니다.
        /// 서버에 패킷은 정상적으로 전송되었다면 true 반환
        /// </summary>
        [JsonIgnore]
        internal bool IsSentRequest
        {
            get { return mIsSentRequest; }
            set { mIsSentRequest = value; }
        }
        bool mIsSentRequest = false;

        /// <summary>
        /// 응답 패킷 수신 여부
        /// </summary>
        [JsonIgnore]
        internal bool IsReceivedResponse
        {
            get { return mIsReceivedResponse; }
            set { mIsReceivedResponse = value; }
        }
        bool mIsReceivedResponse = false;

        private string mHashKey = null;
        private bool mIsFixedHashKey = false;

        /// <summary>
        /// 해쉬키가 설정되어 있지 않다면 입력받은 해쉬 키를 설정합니다.
        /// </summary>
        internal bool UpdateHashKey(string key)
        {
            if (!mIsFixedHashKey)
            {
                mHashKey = key;
                return true;
            }
            else
            {
                return false;
            }
        }

        [JsonIgnore]
        internal string HashKey
        {
            get { return mHashKey; }
        }

        /// <summary>
        /// 패킷에 직접 해쉬 키를 등록하면 본 키를 가지고 패킷을 암호화합니다.
        /// 로그인 없이 진행되는 패킷에 주로 사용합니다. 보통 DB에 저장되어 있는 키를 사용
        /// 서버에서도 특정 패킷에 대해서는 DB에 저장되어 있는 키를 불러와서 복호화합니다.
        /// 이 함수를 사용해서 HashKey를 설정하면 이후 변경되지 않습니다.
        /// </summary>
        protected void SetHashKey(string hashKey)
        {
            mHashKey = hashKey;
            mIsFixedHashKey = true;
        }

        /// <summary>
        /// 패킷 재전송 시도 횟수
        /// </summary>
        [JsonIgnore]
        internal int RetryCount { get { return mRetryCount; } set { mRetryCount=value; } }
        int mRetryCount = 0;

        /// <summary>
        /// 지정한 패킷에 대해 타임 아웃 시간을 직접 설정
        /// </summary>
        [JsonIgnore]
        public int TimeOutMsec { get { return mTimeout; } set { mTimeout = value; } }
        int mTimeout = 0;

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        internal bool IsFirstSending { get { return mRetryCount == 0; } }

        /// <summary>
        /// 재시도 횟수를 초기화
        /// </summary>
        internal void ResetRetryCount ()
        {
            mRetryCount = 1;
        }

        /// <summary>
        /// 재시도 횟수 증가
        /// </summary>
        internal void IncRetryCount()
        {
            mRetryCount++;
        }

        /// <summary>
        /// 커넥션 정보, 중간에 다른 서버로 패킷을 보낼 경우
        /// </summary>
        [JsonIgnore]
        internal IConnectionData Connection { get { return mConnection; } set { mConnection = value; } }
        IConnectionData mConnection = null;

        /// <summary>
        /// 당장 지원은 하지 않지만, 파일을 첨부해서 HTTP 패킷을 보낼 수 있도록 합시다.
        /// 테이블 파일을 보낼때 유리할 것 같습니다.
        /// </summary>
        protected void AttachBinary(byte[] buffer)
        {
            AttchedBinary = buffer;
        }

        byte[] mAttchedBinary = null;

        /// <summary>
        /// 첨부된 바이너리 데이터
        /// </summary>
        [JsonIgnore]
        public byte[] AttchedBinary
        {
            get { return mAttchedBinary; }
            private set { mAttchedBinary = value; }
        }
    }

    /// <summary>
    /// 패킷 파라메터 형식
    /// </summary>
    public enum ParamMode
    {
        Json,
        UrlEncoded
    };

    /// <summary>
    /// 서버에서 패킷을 수신했을 때 패킷 유효성 검사하는 방식
    /// 서버에서 패킷 검사하는 함수 이름으로 정의
    /// </summary>
    public enum PacketVerify
    {
        CheckSession,   // 쿠키로 전달하는 세션값이 존재할 경우 해당 UserID로 전달 (암호화시 암호키는 별도 저장)
        CheckPacket,    // 파라메터의 AccessCode 비교 후 일치할 경우 처리 (암호화시 암호키는 AccessCode)
        PassPacket      // 무조건 통과 (암호화시 암호키는 기본값 사용)
    };

    /// <summary>
    /// RESPONSE_TYPE으로 ResponseType을 반환하도록 구현된 요청 패킷
    /// </summary>
    [System.Reflection.Obfuscation]
    public abstract class RequestPacket<RESPONSE_TYPE> : RequestPacket, IEnumerator, IEnumerator<RESPONSE_TYPE>
        where RESPONSE_TYPE : ResponsePacket
    {
        protected RequestPacket()
        {

        }
        protected RequestPacket(ParamMode paramMode) : this(paramMode, PacketVerify.CheckSession)
        {            
        }

        //protected RequestPacket(CheckMode checkMode) : this(ParamMode.UrlEncoded, checkMode)
        //{            
        //}

        protected RequestPacket(ParamMode paramMode, PacketVerify checkMode)
        {
            ParamMode = paramMode;

            // 서버에서 패킷 유효성 검사를 하지 않습니다.
            // 암호화 키도 모듈 기본값을 사용합니다.
            if (checkMode == PacketVerify.PassPacket)
                SetHashKey("");
        }

        [JsonIgnore]
        public sealed override System.Type ResponseType 
        { 
            get { return typeof(RESPONSE_TYPE); } 
        }

        public RESPONSE_TYPE GetResponse(ResponsePacket resPacket)
        {
            return resPacket as RESPONSE_TYPE;
        }

        string mCachedParam = "";
        
        /// <summary>
        /// 캐싱 된 파라메터 문자열을 반환합니다.
        /// 캐싱 된 문자열이 없으면 문자열을 생성한 후에 캐싱합니다.
        /// </summary>
        string GetCachedParam()
        {
            if (string.IsNullOrEmpty(mCachedParam))
                mCachedParam = IsJsonParam ? GetJsonParam(this) : ReturnEncodedParam();

            return mCachedParam;
        }

        protected void ClearCachedParam()
        {
            mCachedParam = null;
        }
        
        /// <summary>
        /// 파라메터 문자열을 생성합니다.
        /// 패킷을 Send() 할때 미리 생성해 놓고 계속 재활용하는 목적과
        /// MainThread에서만 사용할 수 있는 유니티 명령들을 처리하는게 목적입니다.
        /// </summary>
        /// <returns>만약 null을 반환하면 ParamText에 값이 설정되지 않습니다. </returns>
        protected virtual string ReturnEncodedParam() { return ""; }

        /// <summary>
        /// JSON 형식의 문자열을 파라메터로 사용합니다.
        /// 입력받은 객체는 CachedParam 텍스트에 저장됩니다. (무조건 갱신)
        /// </summary>
        protected string SetJsonParam(object instance)
        {
            ParamMode = ParamMode.Json;
            return GetJsonParam(instance);
        }

        private string GetJsonParam(object instance)
        {
            try
            {
                mCachedParam = JsonConvert.SerializeObject(instance, Formatting.None);
                return mCachedParam;
            }
            catch (System.Exception e)
            {
                return string.Format("{\"Exception\" = \"{0}\"}", e.ToString());
            }
        }

        private RESPONSE_TYPE mResponsePacket = null;

        [JsonIgnore]
        public RESPONSE_TYPE Response { get { return mResponsePacket; } }

        public override void Send(OnResponse onResponse)
        {
            // [2016-02-18] narlamy
            // 파라메터 텍스트를 미리 생성해둡니다. 생성된 문자열을 계속 사용합니다.
            // 백그라운드 쓰래드로 WebRequest에 파라메터 문자열에 접근하기 이전에 매인 쓰래드에서 처리합니다.
            GetCachedParam();

            mResponsePacket = null;
            mState = State.Sent;

            base.Send((response) =>
            {
                mResponsePacket = this.GetResponse(response);
                mState = State.Complete;

                if (onResponse != null)
                    onResponse(response);
            });
        }

        public IEnumerator<RESPONSE_TYPE> Send()
        {
            this.Send(null);
            return this;
        }

        /// <summary>
        /// 서버로부터 응답이 왔는지 여부 (Response 패킷 수신 완료)
        /// 아주 드물지만 다른 타입의 Response 인스턴스가 올수 도 있으므로 mResponsePacket 인스턴스 null 검사보다는
        /// State 검사가 안전하겠습니다.
        /// </summary>
        [JsonIgnore]
        public bool IsDone { get { return mState == State.Complete; } }

        enum State
        {
            Created,
            Sent,
            Complete
        }

        State mState = State.Created;

        public bool MoveNext()
        {
            return mState < State.Complete;
        }

        public void Reset()
        {
            // nothing
        }

        public void Dispose()
        {
            mResponsePacket = null;
        }

        [JsonIgnore]
        public object Current
        {
            get
            {
                return mResponsePacket;
            }
        }

        [JsonIgnore]
        RESPONSE_TYPE IEnumerator<RESPONSE_TYPE>.Current
        {
            get
            {
                return mResponsePacket;
            }
        }

        /// <summary>
        /// GenerateParam() 함수에 의해 생성된 문자열이나 직접 CachedParam에
        /// 설정한 파라메터 문자열을 반환합니다.
        /// </summary>
        [JsonIgnore]
        public sealed override string Parameters { get { return GetCachedParam(); } }
    }
}
