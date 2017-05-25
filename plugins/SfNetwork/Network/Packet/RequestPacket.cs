using System.Collections;
using System.Collections.Generic;
using System;
using LitJson;

namespace SF.Network
{
    public delegate void OnResponse(ResponsePacket resonse);
    //public delegate RequestPromise OnResponsePromise(ResponsePacket resonse, RequestPromise promise);

    /// <summary>
    /// 추상 요청 패킷
    /// 서버에 무엇인가를 요청하거나 전달 함
    /// </summary>
    [System.Reflection.Obfuscation]
    public abstract class RequestPacket : IDisposable, IEnumerator
    {
        public RequestPacket()
        {
        }

        /// <summary>
        /// 패킷 전송 시간
        /// </summary>
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore), JsonConverter(typeof(IsoDateTimeConverter))]
        //internal DateTime TimeStamp { get; set; }

        [JsonIgnore]
        internal DateTime TimeStamp { get; set; }

        public virtual string PacketName { get { return GetType().Name; } }

        [JsonIgnore]
        public abstract Type ResponseType { get; }

        [JsonIgnore]
        internal bool? IsIgnoreConnecting = null;

        [JsonIgnore]
        internal bool? IsIgnoreRetry = null;
                
        /// <summary>
        /// 패킷을 보냅니다.
        /// </summary>
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
        internal int RetryCount { get { return mRetryCount; } set { mRetryCount = value; } }
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
        internal void ResetRetryCount()
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

        public abstract void Dispose();

        /// <summary>
        /// 파라메터가 구현 여부, 파라메터가 없다면 POST가 아닌 GET으로 요청 할 수 있음
        /// </summary>
        internal abstract bool HasParameter { get; }

        /// <summary>
        /// 전송 할 데이터를 바이너리로 전달
        /// </summary>
        internal abstract byte[] GetBytes();

        /// <summary>
        /// 서버로부터 받아온 스트림으로 ResponsePacket을 생성
        /// </summary>
        internal abstract ResponsePacket ParseResponse(System.IO.Stream stream, long streamLength, string contentType="");

        /// <summary>
        /// 커넥션 정보, 중간에 다른 서버로 패킷을 보낼 경우
        /// </summary>
        [JsonIgnore]
        internal Connection Connection { get { return mConnection; } set { mConnection = value; } }
        Connection mConnection = null;

        /// <summary>
        /// 프로토콜 요청을 위해 Sender 객체에 요청합니다.
        /// </summary>
        private void _Send(OnResponse onResponse)
        {
            // [2017-05-24] narlamy
            // 현재는 단일 파이프 라인만 지원합니다.
            // 멀티 파이프 라인을 사용하려면 Connection에 기능을 추가하고
            // 이쪽에서 분기를 타면 될 것 같습니다.

            this._BeforeRequest();
            Sender.Instance.Send(this, onResponse);
        }

        /// <summary>
        /// 패킷 전송 요청 전에 수행되어야 하는 액션이 있을 경우 구현합니다.
        /// </summary>
        protected virtual void _BeforeRequest() { }

        public virtual void Send(OnResponse onResponse)
        {
            this.Reset();

            if (onResponse != null)
                ResponseCallback = onResponse;

            _Send((response) =>
            {
                mResponsePacket = response;
                mState = RequetState.Received;

                if (onResponse != null)
                    onResponse(response);
            });
        }

        /// <summary>
        /// 코루틴을 이용해서 간단히 연속 된 패킷을 처리할 수 있습니다.
        /// 
        /// 응답 패킷을 받았을때 콜백 함수를 호출하는 방식이 아니라 
        /// Send()에서 반환 된 IEnumerator를 이용해서 Response() 맴버 변수에 접근할 수 있습니다.
        /// </summary>
        /// <returns>반환 된 객체가 IsDone일때 </returns>
        public IEnumerator Send()
        {
            this.Reset();

            mState = RequetState.Sent;

            _Send((response) =>
            {
                mResponsePacket = response;
                mState = RequetState.Received;
            });
            return this;
        }

        /// <summary>
        /// 코루틴을 사용하는 방식이 아니라 react 방식을 사용하는 Send()
        /// </summary>
        //public RequestPromise SendPromise()
        //{
        //    return new RequestPromise(this._Send);
        //}

        /// <summary>
        /// 서버로부터 응답이 왔는지 여부 (Response 패킷 수신 완료)
        /// 아주 드물지만 다른 타입의 Response 인스턴스가 올수 도 있으므로 mResponsePacket 인스턴스 null 검사보다는
        /// State 검사가 안전하겠습니다.
        /// </summary>
        [JsonIgnore]
        public bool IsDone
        {
            get { return mState == RequetState.Received; }
        }

        RequetState mState = RequetState.Ignore;
        
        enum RequetState
        {
            Ignore,     // Enumerator를 사용하지 않고 Send() 안에서 바로 콜백을 받아서 처리합니다.
            Sent,       // 
            Received    // 
        }

        public void Reset()
        {
            mResponsePacket = null;
            mState = RequetState.Ignore;
        }

        protected ResponsePacket mResponsePacket = null;
    
        public bool MoveNext()
        {
            return !this.IsDone;
        }

        [JsonIgnore]
        public object Current { get { return this; } }
    }
}
