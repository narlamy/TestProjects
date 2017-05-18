using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System;

namespace N2.Network
{
    public class ImpSender : ISender, IEventReconnection, IServerThread
    {
        Connection mConnector = null;

        public ImpSender(Connection connector)
        {
            mCallbackAgent = ModuleFactory.CreateCallbackAgent();
            StartHttps();

        }

        #region CRITIFICATE
        void StartHttps()
        {
            // Https를 사용하기 위해 서버 인증서 유효성 검사 콜백 등록
            ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallback;
        }

        public bool CertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isSuccess = true;

            // If there are errors in the certificate chain, look at each error to determine the cause.
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new System.TimeSpan(0, 1, 0);
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                        bool chainIsValid = chain.Build((X509Certificate2)certificate);
                        if (!chainIsValid)
                        {
                            isSuccess = false;
                        }
                    }
                }
            }
            return isSuccess;
        }
        #endregion

        #region EVENT
        OnChangeConnectingState mOnChangeLoadingState;

        public OnChangeConnectingState OnChangeConnectingState
        {
            set { mOnChangeLoadingState = value; }
            private get { return mOnChangeLoadingState; }
        }

        event OnReconnect mOnReconnect;
        public event OnReconnect OnReconnect
        {
            add { mOnReconnect += value; }
            remove { mOnReconnect -= value; }
        }
        #endregion

        #region ENVIRONMENT
        // 동시에 Request 요청할 수 있는 최대 개수
        const int MAX_PARELLEL_COUNT = 10;

        // 패킷 요청하고 응답을 기다리는 최대 패킷 수
        const int MAX_RESPONSE_WAITING = 1;

        // 네트웍 오류시 Request,Response 재요청하는 최대 회수
        const int RESENDING_COUNT = 3;

        /// <summary>
        /// 응답 대기 시간 (msec)
        /// </summary>
        const int DEFAULT_TIMEOUT = 7 * 1000;  // 기본값 : 7초

        ulong mPacketID = 0;
        bool mUseEncryption = true;
        int mTimeout = DEFAULT_TIMEOUT;
        int mSecurePortOffset = 10000;

        public int Timeout
        {
            get { return mTimeout; }
            set { mTimeout = value; }
        }

        string mHashKey = null;
        public string HashKey
        {
            private get { return mHashKey; }
            set { mHashKey = value; }
        }

        public int SecurePortOffset
        {
            get { return mSecurePortOffset; }
            set { mSecurePortOffset = value; }
        }

        private string mUrl = "";
        public string URL { set { mUrl = value; } get { return mUrl; } }

        private int mPort = 0;
        public int Port { set { mPort = value; } get { return mPort; } }

        public bool UseEncryption { get { return mUseEncryption; } set { mUseEncryption = value; } }

        public bool IsWriteLog
        {
            get { return mIsWriteLog; }
            set { mIsWriteLog = value; }
        }
        private bool mIsWriteLog = false;

        RequestPacket mLoginPacket = null;
        RequestPacket mLogoutPacket = null;

        OnDisconnect mOnDisconnect = null;
        OnRelogin mOnRelogin = null;
        OnChangeConnectingState mOnChangeLoadingState = null;
        OnPatch mOnPatch = null;
        OnError mOnError = null;
        OnReconnect mOnRecconect = null;

        /// <summary>
        /// 로그인 패킷을 등록해두면, 서버로부터 세션이 만료되었거나
        /// 서버로부터 접속이 끊어졌을 때 자동으로 로그인 처리됩니다.
        /// </summary>
        public RequestPacket LoginPacket { get { return mLoginPacket; } set { mLoginPacket = value; } }

        /// <summary>
        /// 내부에서 에러가 발생 했을 때 에러 처리
        /// 다른 디바이스에서 로그인이 일어나서 서버에서 세션을 끊어버린 경우
        /// </summary>
        public event OnDisconnect OnDisconnect { add { mOnDisconnect += value; } remove { mOnDisconnect -= value; } }

        /// <summary>
        /// 세션 기간이 만료되어서 재로그인이 완료되었을 경우 발생
        /// </summary>
        public event OnRelogin OnRelogin { add { mOnRelogin += value; } remove { mOnRelogin -= value; } }

        /// <summary>
        /// 로그인 Response를 가지고 재연결
        /// </summary>
        public event OnReconnect OnReconnect { add { mOnRecconect += value; } remove { mOnRecconect -= value; } }

        /// <summary>
        /// 패킷 에러 발생시 발생하는 이벤트
        /// </summary>
        public event OnError OnError { add { mOnError += value; } remove { mOnError -= value; } }

        /// <summary>
        /// 패치 단계로 이동해야 합니다.
        /// </summary>
        public event OnPatch OnPatch { add { mOnPatch += value; } remove { mOnPatch -= value; } }

        /// <summary>
        /// 로그 아웃 패킷을 등록해두면, 웹서버 인스턴스가 Dispose 될때
        /// 자동으로 로그아웃 요청을 보냅니다.
        /// </summary>
        public RequestPacket LogoutPacket { get { return mLogoutPacket; } set { mLogoutPacket = value; } }

        QueryRetransmit mQueryRetransmit = null;
        public QueryRetransmit QueryRetransmit
        {
            get
            {
                if (mQueryRetransmit == null)
                    mQueryRetransmit = (p, r) => { r(false); };
                return mQueryRetransmit;
            }
            set { mQueryRetransmit = value; }
        }

        SyncServerData mOnSyncServer = null;
        public SyncServerData SyncServerData
        {
            get
            {
                if (mOnSyncServer == null)
                    mOnSyncServer = (callback) => { callback(true); };
                return mOnSyncServer;
            }
            set { mOnSyncServer = value; }
        }

        /// <summary>
        /// 쿠키~쿠키~ 헤에~ 조앙... 조앙.. (^▽^*)
        /// Request때 마다 sessionID를 따로 포함해서 보낼 필요가 없당.
        /// </summary>
        private CookieContainer mCookieContainer = new CookieContainer();

        /// <summary>
        /// 1msec는 몇 ticks 일까?
        /// </summary>
        const int MSEC_TO_TICKS = 10000;

        /// <summary>
        /// 압축 기준 크기, 지정한 크기(bytes)보다 클 경우 패킷을 압축합니다.
        /// </summary>
        public uint CompressThreshold
        {
            get { return mCompressThreshold; }
            set { mCompressThreshold = value; }
        }

        uint mCompressThreshold = 512;

        /// <summary>
        /// 일정 용량을 초과할 경우 압축해서 전송하는 기능 지원 여부
        /// </summary>
        public bool SuportCompression
        {
            get { return mSuportCompression; }
            set { mSuportCompression = value; }
        }

        bool mSuportCompression = true;
        #endregion

        #region THREAD
        Thread mResponseThread = null;
        ICallbackAgent mCallbackAgent = null;

        /// <summary>
        /// 패킷 비동기 처리를 위한 쓰래드를 시작합니다.
        /// </summary>
        /// <returns>비동기 모드면 true를 반환, 동기 모드면 false 반환</returns>
        public bool Run()
        {
            try
            {
                //if (mCallbackAgent == null || !mNetResponseProxy.IsPlaying)
                //{
                //	NetLogger.Instance.LogWarning("쓰래드를 사용하지 않아서 무시됩니다.");
                //	return false;
                //}

                if (mResponseThread == null)
                {
                    mResponseThread = new Thread(BackgroundProcess_);
                    //Logger.Instance.Log("[WebServer] Run() => create");
                }

                if (mResponseThread.ThreadState != ThreadState.Running)
                {
                    mResponseThread.Start();
                    //Logger.Instance.Log("[WebServer] Run() => start thread");
                }
                return true;
            }
            catch (System.Exception e)
            {
                NetLogger.Instance.LogError("[WebServer] Run() => ", e.ToString());
                return false;
            }
        }
        #endregion

        #region  RELEASE

        /// <summary>
        /// 어플리케이션이 종료되는 시점에서 필요한걸 정리합니다.
        /// 로그아웃 패킷보내고, GameObject와의 연결도 끊으며
        /// 각종 패킷 큐들을 초기화합니다.
        /// </summary>
        void QuitApp_(bool sendLogOut)
        {
#if DEBUG
            if (UnityEngine.Application.isEditor)
                NetLogger.Instance.Log("[WebServer] Quit(\"SndLogOut={0}\")", sendLogOut);
#endif
            // [2015-03-22] narlamy
            // 로그 아웃 패킷 보내기 (미스테리)
            // 로그 아웃 패킷을 안 보내면, 에디터에서 다시 플레이하면 프리징 현상 발생
            // 로그 아웃이 아닌 다른 패킷을 보내도 프리징 현상 안 생김
            // 패킷을 보내야 함. 패킷을 보내야 함.
            //if(sendLogOut)
            //    Logout();

            // 쿠키 초기화
            ResetCookie_();

            // 쓰래드를 강제 종료하기 때문에 미처 못 보낸 패킷은
            // 서버로 보내지 않고 그대로 종료합니다.
            if (mResponseThread != null)
            {
                mResponseThread.Abort();
                mResponseThread = null;
            }

            // MainThread에서 콜백 처리를 위한 객체 연결 초기화
            mNetResponseProxy = null;

            // 모든 큐를 비웁니다.
            ClearAllQueue();

            // 연결 된 패킷 초기화
            mLoginPacket = null;
            mLogoutPacket = null;

            // 연결된 이벤트(delegate) 초기화
            mOnDisconnect = null;
            mOnRelogin = null;
            mOnChangeLoadingState = null;
            mOnPatch = null;
            mOnError = null;
            mOnSyncServer = null;
            mQueryRetransmit = null;
        }

        void ClearAllQueue()
        {
            lock (mQueueReserved)
                mQueueReserved.Clear();
        }

        void ResetCookie_()
        {
            mCookieContainer = new CookieContainer();
        }
        #endregion

        #region SEND / REQUEST

        // 전송 요청 큐
        LinkedList<SendingReqInfo> mQueueReserved = new LinkedList<SendingReqInfo>();

        // 전송 대기 큐
        List<SendingReqInfo> mQueueWaiting = new List<SendingReqInfo>();

        // 재시도 큐
        List<SendingReqInfo> mQueueRetry = new List<SendingReqInfo>();

        void RemoveFromWaiting(RequestPacket reqPacket)
        {
            lock (mQueueWaiting)
            {
                var index = mQueueWaiting.FindIndex(info => info.RequestPacket == reqPacket);
                if (index >= 0)
                    mQueueWaiting.RemoveAt(index);
            }
        }

        void RemoveFromRetry(RequestPacket reqPacket)
        {
            lock (mQueueRetry)
            {
                var index = mQueueRetry.FindIndex(info => info.RequestPacket == reqPacket);
                if (index >= 0)
                    mQueueRetry.RemoveAt(index);
            }
        }

        void RemoveFromReserved(RequestPacket reqPacket)
        {
            lock (mQueueReserved)
            {
                var node = mQueueReserved.First;
                while (node != null)
                {
                    if (node.Value.RequestPacket == reqPacket)
                    {
                        mQueueReserved.Remove(node);
                        node = null;
                    }
                    else
                    {
                        node = node.Next;
                    }
                }
            }
        }


        /// <summary>
        /// 패킷 요청 정보
        /// RequestPacket과 OnResponse 콜백 정보를 가지고 있습니다.
        /// </summary>
        struct SendingReqInfo
        {
            public RequestPacket RequestPacket;
            public OnResponse OnResponse;
            public System.DateTime SendingTime;

            public SendingReqInfo(RequestPacket request, OnResponse onResponse, int delayTime = 0)
            {
                // [2015-03-22] narlamy
                // 당장은 INetResponseProxy 인스턴스를 사용하고 있지않지만
                // 코드를 삭제하지마세요. 코드를 삭제하면 정상 동작되지 않습니다.
                // 최초 패킷을 보내는 시점에서 NetResponseProxy 객체가 만들어지고
                // 쓰래드가 생성될 것 입니다.

                RequestPacket = request;
                OnResponse = onResponse;

                var now = System.DateTime.Now;

                if (delayTime > 0)
                    SendingTime = new System.DateTime(now.Ticks + delayTime * MSEC_TO_TICKS);
                else
                    SendingTime = now;
            }

            public bool IsSendable
            {
                get { return System.DateTime.Now > SendingTime; }
            }
        }

        public void Send(RequestPacket reqPacket, OnResponse onResponse)
        {
            // [2016-03-22] narlamy
            // 기본 커낵션 정보가 아닌 별도의 커낵션 정보를 사용한다면 패킷에 연결
            if (!IsDefaultConnection)
                reqPacket.Connection = new ConnectionData(this);

            Send_(reqPacket, onResponse, !NetResponseProxy.IsPlaying);
        }

        void Send_(RequestPacket requestPacket, OnResponse onResponse, bool Immediate)
        {
            // 패킷 인스턴스가 null이면 에러 처리
            if (requestPacket == null)
            {
                NetLogger.Instance.LogError("[WebServer] Packet.Send() => 요청 패킷 인스턴스가 null");

                if (onResponse != null)
                    onResponse(requestPacket.CreateResponse());
                return;
            }

            // 네트웍 모듈이 꺼져 있을때 바로 에러 코드 반환
            if (!IsActivation)
            {
                if (onResponse != null)
                    onResponse(requestPacket.CreateResponse(ServerError.Disabled));
                return;
            }

            // URL 검사
            if (mUrl == "" || !HttpListener.IsSupported)
            {
                NetLogger.Instance.LogError(string.Format("[WebServer] URL error or not supported (Url=\"{0}\", Support={1})",
                    mUrl, HttpListener.IsSupported));

                CallResponse_(onResponse, requestPacket.CreateResponse(ServerError.Unknown));
                return;
            }

            // 전송
            if (Immediate)
            {
                Request_(requestPacket, onResponse);
            }
            else
            {
                // 패킷 요청을 순차적으로 진행시키기 위해서 사용합니다.
                // 너무 많은 요청이 응답 받기 전에 들어왔을때 오버해드나
                // 오동작이 일어나는 것을 방지합니다.

                lock (mQueueReserved)
                    mQueueReserved.AddLast(new SendingReqInfo(requestPacket, onResponse));
            }
        }

        /// <summary>
        /// RequestPacket을 가지고 WebRequest 인스턴스를 생성하고 
        /// HTML Body에 파라메터 정보를 기록합니다.
        /// 여기서 패킷 암호화도 이뤄집니다.
        /// </summary>
        void Request_(RequestPacket reqPacket, OnResponse onResponse)
        {
            WebSocket socket = WebSocket.Empty;

            try
            {
                // 서버에 패킷을 전송합니다.
                socket = SendReqPacket_(reqPacket, onResponse);

                // 서버로 부터 응답을 기다리고 응답 된 패킷을 onResponse 콜백을 통해 전달합니다.
                ReceiveResPacket_(socket, reqPacket, onResponse);
            }
            catch (System.TimeoutException e)
            {
                var msg = "[WebServer] retry reqeust : " + e;
                NetLogger.Instance.LogWarning(msg);

                if (reqPacket.IsSentRequest && socket.IsValidInatnce && CanResend(reqPacket))
                {
                    // [2016-03-29] narlamy
                    // 이미 패킷은 전송했는데 타임 아웃이 걸린 경우
                    // 클라이언트가 서버로부터 응답이 끝난 상태이기 때문에 
                    // 응답 요청을 다시 시도해 보겠습니다.

                    ReceiveResPacket_(socket, reqPacket, onResponse);
                }
                else
                {
                    // 타임 아웃인 경우 단순히 패킷 전송 재시도 합니다.
                    // 재시도 횟수 초과시 에러 처리됩니다.
                    // 대부분의 타임 아웃은 여기서 처리됩니다.

                    RetryRequest_(reqPacket, onResponse);
                }
            }
            catch (System.Net.WebException e)
            {
                // HTTP 에러 처리
                WebError(e, reqPacket, onResponse);
            }
            catch (System.Exception e)
            {
                GeneralError(e, reqPacket, onResponse);
            }
            finally
            {
                // 패킷 처리 완료
                // 소켓 반환, 다음 전송 요청하기

                //if (webRequest != null)
                //    webRequest.Abort();

                socket.Dispose();
            }
        }

        private bool mAlwaysSSL = false;
        public bool AlwaysSSL { get { return mAlwaysSSL; } set { mAlwaysSSL = value; } }

        /// <summary>
        /// 요청 패킷을 전송하고 WebRequest 객체를 반환합니다.
        /// </summary>
        WebSocket SendReqPacket_(RequestPacket reqPacket, OnResponse onResponse)
        {
            WebSocket socket = WebSocket.Empty;

            var packetName = reqPacket.PacketName;
            var packetParam = reqPacket.Parameters;
            var reqPacketType = reqPacket.GetType();
            var packetGroup = System.Attribute.GetCustomAttribute(reqPacketType, typeof(PacketGroupAttribute)) as PacketGroupAttribute;
            var packetRevision = System.Attribute.GetCustomAttribute(reqPacketType, typeof(PacketRevisionAttribute)) as PacketRevisionAttribute;

            var connection = reqPacket.Connection;
            var url = connection != null ? connection.URL : this.URL;
            var port = connection != null ? connection.Port : this.Port;
            var useEncrypt = connection != null ? connection.UseEncryption : UseEncryption;
            var timeout = connection != null ? connection.Timeout : Timeout;

            var isSecurity = reqPacket.IsSecurity || AlwaysSSL;

            if (isSecurity)
                url = url.ToLower().Replace("http:", "https:");

            // 첫전송 요청일때 커넥팅 상태를 활성화시킵니다.
            // 콜백 처리가 완료되는 시점에 커넥팅 상태가 비활성화되며 재전송 요청이 들어오는
            // 경우에는 케니곁이 상태를 그대로 유지하게 되므로 재전송 상황에서는 무시합니다.
            PostConnectingState(reqPacket);

            // 패킷에 해쉬키가 별도로 설정되어 있지 않다면 로그인시 입력받은 해쉬키를 설정합니다.
            // Response 패킷을 디코딩할때 사용합니다.
            reqPacket.UpdateHashKey(HashKey);

            // 접속 URL
            var uriString = string.Format("{0}:{1}/{2}{3}{4}?pid={5}&p={6}",
                url,
                port + (isSecurity ? SecurePortOffset : 0),
                (packetGroup != null) ? packetGroup.UriGroupName : "",
                packetName,
                (packetRevision != null) ? ("/" + packetRevision.Revision) : "",
                reqPacket.IsFirstSending ? ++mPacketID : mPacketID,
                string.IsNullOrEmpty(packetParam) ? "n" : "y"
            );

            // WebRequest 생성 (WebSocket 오브젝트로 관리)
            socket = SocketFactory.CreateSocket(
                uriString,
                reqPacket.IsJsonParam,
                reqPacket.TimeOutMsec > 0 ? reqPacket.TimeOutMsec : timeout,
                mCookieContainer
            //connection == null
            );

            // Request 패킷에 파라메터가 있는 경우 POST 방식으로 전송
            if (!string.IsNullOrEmpty(packetParam))
            {
                // 전송 시간
                //reqPacket.TimeStamp = System.DateTime.UtcNow;

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

                // body에 파라메터 저장
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(packetParam);
                socket.WriteData(bytes, SuportCompression, CompressThreshold);
            }

            // [2016-03-29] narlamy
            // POST 방식일때는 ContentLength만큼 request stream에 데이터가 저장이 완료되면 패킷 발송
            // GET 방식일때는 GetResponse() 호출시 패킷 발송
            reqPacket.IsSentRequest = true;

            // 디버깅 로그
            WriteRequestLog(reqPacket);

            return socket;
        }

        /// <summary>
        /// Response 패킷을 수신합니다.
        /// </summary>
        void ReceiveResPacket_(WebSocket socket, RequestPacket reqPacket, OnResponse onResponse)
        {
            if (!socket.IsValidInatnce)
            {
                CallResponse_(onResponse, reqPacket.CreateResponse(ServerError.NullInstance));
                return;
            }

            using (var webResponse = socket.GetResponse())
            {
                // 서버로 부터 응답이 완료된 상태
                // 서버의 응답을 받지 못하고 아래 exception 크드로 빠질 수 있습니다.
                // 패킷 재전송 여부 판단은 아래 플래그를 이용해서 합니다.                    
                reqPacket.IsReceivedResponse = true;

                if (webResponse != null)
                {
                    // 응답 받은 패킷을 가지고 ResponsePacket 클래스를 생성합니다.
                    using (var webResponseStream = webResponse.GetResponseStream())
                    {
                        var resPacket = GenerateResponsePacket_(reqPacket, webResponseStream);
                        WriteReceiveLog(resPacket);

                        CallResponse_(onResponse, resPacket);
                    }

                    // using() 블록 벗어나면서 Dipose()에 의해 자동으로 Close() 될 것 같지만
                    // 안전하게 Close()를 명시합니다.
                    webResponse.Close();
                }
                else
                {
                    // [2016-08-19] narlamy
                    // HttpWebResponse를 왜 못 만들까?

                    CallResponse_(onResponse, reqPacket.CreateResponse(ServerError.InternalError));
                }
            }
        }
		#endregion


		#region 응답 패킷 생성 및 로그
		/// <summary>
		/// 응답받은 패킷의 내용을 로그로 출력합니다.
		/// </summary>
		void WritePacketLog_(RequestPacket reqPacket, HttpStatusCode statusCode, string content)
		{
			NetLogger.Instance.LogWarning("[WebServer] Response error (Status={1}({2}), RequestPacket=\"{3}\") : \"{0}\"",
				content, statusCode, (int)statusCode, reqPacket.GetType().Name);
		}

		/// <summary>
		/// 응답받은 패킷을 내용을 가지고 적절한 ResponsePacket 인스턴스를 생성합니다.
		/// </summary>
		ResponsePacket GenerateResponsePacket_(RequestPacket request, Stream responseStream)
		{
			if (responseStream != null)
			{
				using (TextReader reader = new StreamReader(responseStream))
				{
					string content = reader.ReadToEnd();
					return GenerateResponsePacket_(request, content);
				}
			}
			else
			{
				return request.CreateResponse(ServerError.StreamError);
			}
		}

		ResponsePacket GenerateResponsePacket_(RequestPacket request, string content, int contentEmptyError = ServerError.ParsingError)
		{
			if (string.IsNullOrEmpty(content))
			{
				NetLogger.Instance.Log("[WebServer] responseStream에서 문자열을 얻어 올 수 없습니다. (ReqType=\"" + request.GetType().Name + "\")");
				return request.CreateResponse(contentEmptyError);
			}
			else
			{
				// 정상적으로 진행
				return request.CreateResponse(content);
			}
		}
		#endregion

		#region ERROR
		/// <summary>
		/// 일반적인 에러
		/// </summary>
		void GeneralError(System.Exception e, RequestPacket reqPacket, OnResponse onResponse)
        {
            if (IsWriteLog)
            {
                var msg = string.Format("[WebServer] GeneralError (\"{1}\", URL=\"{2}:{3}\") : {0}", e, (reqPacket != null) ? reqPacket.PacketName : "<<null>>", mUrl, mPort);
                NetLogger.Instance.LogWarning(msg);
            }

            if (reqPacket != null)
            {
                // 유저한테 패킷 재전송 여부 확인 후 처리 (대화 팝업창 출력)
                QueryRetransmit_(reqPacket, onResponse);
            }
            else
            {
                // 커넥팅 상태 해제
                //if (!IsHideConnecting)
                //    ReqHideConnecting();

                // [2016-03-21] narlamy
                // RequestPacket이 null이네요.
                // ResponsePacket은 ErrorResponse를 생성해서 전달합니다.

                NetLogger.Instance.LogError("[WebServer] RequestPacket 인스턴스가 null 입니다.");

                CallResponse_(onResponse, reqPacket.CreateResponse());
                //ReleaseReqTrigger_(reqPacket);
            }
        }

        void WebError(System.Net.WebException e, RequestPacket reqPacket, OnResponse onResponse)
        {
            if (IsWriteLog)
            {
                string msg = string.Format("[WebServer] WebError (\"{1}\") : {0}", e, reqPacket.PacketName);
                NetLogger.Instance.LogWarning(msg);
            }

            // 전송한 후에 에러가 발생했다면 
            //if (reqPacket.IsSentRequest && !reqPacket.IsReceivedResponse && !IsHideConnecting)
            //    ReqHideConnecting();

            if (e.Response is HttpWebResponse)
            {
                using (HttpWebResponse httpResponse = e.Response as HttpWebResponse)
                {
                    HttpStatusCode statusCode = httpResponse.StatusCode;
                    ResponsePacket responsePacket = null;

                    using (TextReader reader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        string content = reader.ReadToEnd();

                        switch (statusCode)
                        {
                            case HttpStatusCode.Unauthorized:   // 401 응답 (권한 없음, 로그인 필요)
                                responsePacket = reqPacket.CreateResponse(ServerError.Unauthorized, content);
                                CallResponse_(onResponse, responsePacket);
                                break;

                            case HttpStatusCode.NotAcceptable:  // 406 응답
                                                                // 서버에서 보내온 값을 그대로 사용, 만약 파싱 실패가 되면 LogOut 에러로 처리
                                responsePacket = GenerateResponsePacket_(reqPacket, content, ServerError.LogOut);
                                CallResponse_(onResponse, responsePacket);
                                break;

                            case HttpStatusCode.BadRequest:         // 400 응답
                            case HttpStatusCode.NotImplemented:     // 501 응답
                                responsePacket = reqPacket.CreateResponse(ServerError.InvalidRequest, content);
                                CallResponse_(onResponse, responsePacket);
                                break;

                            case HttpStatusCode.Forbidden:  // 403 응답
                                responsePacket = GenerateResponsePacket_(reqPacket, content);

                                if (responsePacket.GetErrorCode() == ServerError.None)
                                    responsePacket.SetErrorCode(ServerError.Reject);

                                CallResponse_(onResponse, responsePacket);
                                WritePacketLog_(reqPacket, statusCode, content);
                                break;

                            case HttpStatusCode.MethodNotAllowed:   // 405 응답       
                            case HttpStatusCode.InternalServerError:// 500 응답
                                                                    // 원래 패킷 형태 그대로 보내고 에러값만 다르게 보낼때 사용
                                responsePacket = GenerateResponsePacket_(reqPacket, content);
                                CallResponse_(onResponse, responsePacket);
                                break;

                            case HttpStatusCode.NotFound:   // 404 응답
                                responsePacket = GenerateResponsePacket_(reqPacket, content, ServerError.NotFoundRequest);
                                CallResponse_(onResponse, responsePacket);
                                WritePacketLog_(reqPacket, statusCode, content);
                                break;

                            default:  // 403 응답 포함
                                responsePacket = GenerateResponsePacket_(reqPacket, content);
                                CallResponse_(onResponse, responsePacket);
                                WritePacketLog_(reqPacket, statusCode, content);
                                break;
                        }
                    }

                    // WaitResonse_()를 따로 호출하지 않으므로 반드시
                    // 초기화시켜줘야 합니다.
                    //ReleaseReqTrigger_(reqPacket);
                }
            }
            else
            {
                switch (e.Status)
                {
                    case WebExceptionStatus.ConnectFailure:
                    case WebExceptionStatus.ConnectionClosed:
                        // 서버로부터 접속이 끊어졌습니다.
                        // [2014-07-24]
                        // 요청한 패킷을 큐에 담아두고 계속 시도해야 합니다.
                        // 새로운 요청 패킷을 보내는 것이 아니라 응답을 기다립니다.

                        RetryRequest_(reqPacket, onResponse);
                        break;

                    default:
                        //NetLogger.Instance.LogError("[WebServer] Response_() : web socket error (Status={0}, ReqPacket=\"{1}\")",
                        //    e.Status, reqPacket.GetType().Name);

                        // 유저한테 재전송 문의
                        QueryRetransmit_(reqPacket, onResponse);
                        break;
                }
            }
        }
        #endregion

        #region CONNECTION_ERROR handling
        /// <summary>
        /// 등록된 콜백을 통해 사용자한테 재전송 유무를 물어 본 후에
        /// 수락을 할 경우 재전송하며, 거절할 경우 실패 패킷으로 처리합니다.
        /// </summary>
        void QueryRetransmit_(RequestPacket reqPacket, OnResponse onResponse)
		{
			// [2016-06-13] narlamy
			// 현재 상태와 상관없이 Hide 이벤트를 보내서 커넥팅 상태를 변경합니다.
			PostConnectingState(reqPacket, true);

			// 유저한테 재전송 여부 확인
			NetResponseProxy.PushCallback(QueryRetransmit, reqPacket, (isRetransmit) =>
			{
				if (isRetransmit)
				{
					// 재시도 카운트 초기화
					reqPacket.ResetRetryCount();

					// [2016-02-18] narlamy
					// 큐에 담아서 백그라운드 쓰래드를 통해 Request 처리되도록 합니다.
					RetryRequest_(reqPacket, onResponse, true);
				}
				else
				{
					ResponsePacket resPacket = reqPacket.CreateResponse(ServerError.NetworkError);
					CallResponse_(onResponse, resPacket);
				}
			});
		}

		/// <summary>
		/// 자동 패킷 요청 횟수가 초과한 경우 유저한테 재전송 여부를 물어봅니다.
		/// </summary>
		bool CanResend(RequestPacket reqPacket)
		{
			return reqPacket.RetryCount < RESENDING_COUNT || reqPacket.IsIgnoreRetry();
		}

		/// <summary>
		/// 일정 주기로 패킷을 재전송하도록 합니다.
		/// 정확히는 WebRequest를 새로 생성합니다.
		/// </summary>
		void RetryRequest_(RequestPacket reqPacket, OnResponse onResponse, bool isImmediatley = false)
		{
			if (!CanResend(reqPacket))
			{
				// 재시도 카운트를 초기화
				reqPacket.ResetRetryCount();

				// [2016-02-17] narlamy
				// 재전송 횟수를 초과하였기 떄문에 유저한테 재전송 여부를 물어보도록 합니다.
				// 유저가 재전송을 거부하면 그대로 종료

				QueryRetransmit_(reqPacket, onResponse);
			}
			else
			{
				// 재시도 카운팅
				reqPacket.IncRetryCount();

				// 대기 큐에서는 제거
				RemoveFromWaiting(reqPacket);

				// 재시도 큐에 삽입
				lock (mQueueRetry)
					mQueueRetry.Add(new SendingReqInfo(reqPacket, onResponse, isImmediatley ? 0 : Timeout));
			}
		}

		/// <summary>
		/// 등록한 Response 패킷을 콜백 함수를 통해 전달합니다.
		/// </summary>
		private class Responser : IResponser
		{
			OnResponse mOnResponse;
			ResponsePacket mResponsePacket;

			public Responser(OnResponse callback, ResponsePacket packet)
			{
				mOnResponse = callback;
				mResponsePacket = packet;
			}

			public void Response()
			{
				if (mOnResponse != null)
					mOnResponse(mResponsePacket);
			}
		}

		/// <summary>
		/// 모든 큐에서 RequestPakcet을 제거
		/// </summary>
		void RemoveRequestPacket(RequestPacket reqPacket)
		{
			RemoveFromWaiting(reqPacket);
			RemoveFromReserved(reqPacket);
			RemoveFromRetry(reqPacket);
		}

		private bool mIsIgnoreCallbackOnDisconnet = false;
		public bool IsInterruptCallbackOnDisconnet { get { return mIsIgnoreCallbackOnDisconnet; } set { mIsIgnoreCallbackOnDisconnet = value; } }

		#endregion

		#region 콜백 및 이벤트 처리        
		/// <summary>
		/// 입력받은 callback 함수를 호출합니다.
		/// 비동기 방식일 경우 NetResponseProxy를 통해 매인 쓰래드를 통해서 호출하며
		/// 동기 방식을 경우는 바로 콜백을 호출합니다.
		/// </summary>
		void CallResponse_(OnResponse onResponse, ResponsePacket resPacket)
		{
			var reqPacket = resPacket.GetRequestPacket();

			// 모든 큐에서 요청 패킷 제거
			RemoveRequestPacket(reqPacket);

			// 패킷 수신 완료 및 커넥팅 상태 변경 이벤트 발생
			PostConnectingState(reqPacket);

			// 일부 특별한 에러에 대해서는 Response 콜백을 호출하지 않고 전용 이벤트를
			// 발생시킵니다.
			if (!resPacket.IsSuccess)
			{
				// [2016-02-25] narlamy
				// 바로 리턴하기 때문에 커넥팅 상태 처리를 직접해야 합니다.
				// 아래 OnReglogin_(), OnPath_() 함수 안에서 구현 필수!!!

				var errorCode = resPacket.GetErrorCode();

				switch (errorCode)
				{
					case ServerError.LogOut:    // 세션 만료로 인한 로그 아웃
						OnLogout_(LogoutReason.SessionTimeout, resPacket.GetRequestPacket(), onResponse);
						return;

					case ServerError.OtherLogin:    // 다른 디바이스에서 로그인
													// 이벤트 콜백이 연결되어 있지 않다면 에러 코드를 Request 콜백에 전달합니다.
						if (OnOtherLogin_(LogoutReason.OtherLogin, resPacket.GetRequestPacket(), onResponse))
							return;
						break;

					case ServerError.RequirePatch:
						// 이벤트 콜백이 연결되어 있지 않다면 에러 코드를 Request 콜백에 전달합니다.
						if (OnPatch_(PatchReason.GameTablePatch, resPacket))
							return;
						break;

					case ServerError.InvalidVersion:
						// 이벤트 콜백이 연결되어 있지 않다면 에러 코드를 Request 콜백에 전달합니다.
						if (OnPatch_(PatchReason.AppPatch, resPacket))
							return;
						break;

					case ServerError.Unauthorized:
						// 계정 정보가 유효하지 않습니다.
						if (OnDisconnect_(DisconnectReason.Unauthorized, resPacket))
							return;
						break;

					case ServerError.ClosedServer:
						// 이벤트 콜백이 연결되어 있지 않다면 에러 코드를 Request 콜백에 전달합니다.
						if (OnDisconnect_(DisconnectReason.ClosedServer, resPacket))
							return;
						break;

					case ServerError.Blocked:
						// 블락 된 계정
						if (OnDisconnect_(DisconnectReason.Blocked, resPacket))
							return;
						break;

					case ServerError.Freezing:
						// 잠시 접속 차단 (킥 오프 상태)
						if (OnDisconnect_(DisconnectReason.Freezing, resPacket))
							return;
						break;
				}
			}

			// Request 패킷 콜백을 호출합니다.
			// 쓰래드를 사용하느냐 안 하느냐에 따라 콜백 호출 방식이 다릅니다.
			if (onResponse != null)
			{
				if (UseThread)
				{
					var responser = new Responser(onResponse, resPacket);
					NetResponseProxy.PushResponser(responser);
				}
				else
				{
					onResponse(resPacket);
				}
			}

			// Request 패킷 콜백을 처리하고 추가로 이벤트를 발생시킵니다.
			if (!resPacket.IsSuccess)
			{
				// 순수 에러 이벤트
				var error = resPacket.GetError();

				// 에러 이벤트
				if (!resPacket.IsIgnoreErrorEvent())
					OnError_(error, resPacket);

				// 연결 해제 이벤트 처리
				// OnDisconnect 이벤트에 콜백 함수가 연결 되어 있지 않은 경우에도 상태를 진행시키기 위해서 사용합니다.
				switch (error.ErrorCode)
				{
					case ServerError.InvalidVersion:
						OnDisconnect_(DisconnectReason.InvalidVersion, resPacket);
						break;

					case ServerError.Unauthorized:
						OnDisconnect_(DisconnectReason.Unauthorized, resPacket);
						break;

					case ServerError.ClosedServer:
						OnDisconnect_(DisconnectReason.ClosedServer, resPacket);
						break;

					case ServerError.Blocked:
						OnDisconnect_(DisconnectReason.Blocked, resPacket);
						break;

					case ServerError.Freezing:
						OnDisconnect_(DisconnectReason.Freezing, resPacket);
						break;

						//case ServerError.NetworkError:
						//    OnDisconnect_(DisconnectReason.ClosedServer, resPacket);
						//    break;
				}
			}
		}

		ConnectingState mConnectingState = ConnectingState.HideConnecting;

		private bool IsHideConnecting
		{
			get { return mConnectingState == ConnectingState.HideConnecting; }
		}

		/// <summary>
		/// 현재 대기중인 패킷이 있는지 없는지에 따라 커넥팅 표시
		/// </summary>
		void PostConnectingState(RequestPacket reqPacket, bool ignore = false)
		{
			if (mQueueWaiting.Count > 0)
			{
				int count = 0;

				// 현재 요청을 보내서 응답 대기 중인 패킷이 있다면 connecting 표시
				for (int i = 0; i < mQueueWaiting.Count; i++)
				{
					var info = mQueueWaiting[i];

					if (!ignore || reqPacket != info.RequestPacket)
					{
						if (!info.RequestPacket.IsIgnoreConnecting())
							count++;
					}
				}

				mConnectingState = count > 0 ? ConnectingState.ShowConnecting : ConnectingState.HideConnecting;
			}
			else
			{
				mConnectingState = ConnectingState.HideConnecting;
			}

			// 이벤트 전달
			if (OnChangeConnectingState != null)
			{
				if (UseThread)
				{
					NetResponseProxy.PushCallback(OnChangeConnectingState, mConnectingState, reqPacket);
				}
				else
				{
					OnChangeConnectingState(mConnectingState, reqPacket);
				}
			}
		}

		void OnError_(ServerError error, ResponsePacket resPacket)
		{
			if (mOnError == null)
				return;

			if (UseThread)
				NetResponseProxy.PushCallback(mOnError, error, resPacket);
			else
				mOnError(error, resPacket);
		}

		/// <summary>
		/// Disconnect 이벤트 발생
		/// </summary>
		bool OnDisconnect_(DisconnectReason reason, ResponsePacket resPacket)
		{
			// 이벤트 발생
			if (mOnDisconnect == null)
				return false;

			if (UseThread)
				NetResponseProxy.PushCallback(mOnDisconnect, reason, resPacket);
			else
				mOnDisconnect(reason, resPacket);

			return IsInterruptCallbackOnDisconnet;
		}

		/// <summary>
		/// 다른 디바이스에서 로그인했다면 자동으로 패킷을 보내지 않고 로그 아웃 처리합니다.
		/// 이 부분은 정책에 의해 달라지는 부분입니다.
		/// </summary>
		bool OnOtherLogin_(LogoutReason reason, RequestPacket requestPacket = null, OnResponse onResponse = null)
		{
			// 이벤트 발생
			var resPacket = requestPacket.CreateResponse(ServerError.OtherLogin);
			return OnDisconnect_(DisconnectReason.OtherLogin, resPacket);
		}

		// 패치 이벤트 발생
		bool OnPatch_(PatchReason reason, ResponsePacket resPacket)
		{
			if (mOnPatch == null)
				return false;

			if (UseThread)
				NetResponseProxy.PushCallback(mOnPatch, reason, resPacket);
			else
				mOnPatch(reason, resPacket);

			return IsInterruptCallbackOnDisconnet;
		}

		/// <summary>
		/// 로그인 패킷을 보내서 연결을 유지하고 실패 처리되었던 패킷을 다시 보냅니다.
		/// 다른 디바이스에서 로그인 되었다면 자동으로 패킷을 보내지 않고 Disconnect 처리합니다.
		/// </summary>
		void OnLogout_(LogoutReason reason, RequestPacket requestPacket = null, OnResponse onResponse = null)
		{
			// [2016-01-29] narlamy
			// 로그인 패킷을 보냈는데, 서버 에러로 Logout이 온다면 
			// 서버로 부터 거부된 것이기 때문에 더 이상 시도하지 않도록 합니다.

			if (LoginPacket != null && requestPacket != LoginPacket)
			{
				// [2016-03-22] narlamy
				// 패킷을 다시 보내기 때문에 ConnectingCounter를 신경 쓸 필요가 없습니다.

				// 로그인 패킷을 보냅니다.
				LoginPacket.Send((responsePacket) =>
				{
					if (responsePacket.IsSuccess)
					{
						// OnReconnect 이벤트 처리 (매인 쓰래드를 보장하지 않음)
						if (mOnRecconect != null)
							mOnRecconect(responsePacket);

						// 패킷 재전송
						if (requestPacket != null)
						{
							requestPacket.Send(onResponse);
						}
						else
						{
							NetLogger.Instance.LogWarning("[Relogin] requset pakcet is null");
						}

						// OnRelogin 이벤트 처리
						// OnConnection과 달리 UI 등 매인 쓰래드에서 동작하는 걸 보장

						if (mOnRelogin != null)
						{
							if (UseThread)
								NetResponseProxy.PushCallback(mOnRelogin, reason, responsePacket);
							else
								mOnRelogin(reason, responsePacket);
						}
					}
					else
					{
						// 로그인 패킷 모든 큐에서 제거
						RemoveRequestPacket(LoginPacket);

						// 에러 처리 : 자동 로그인에 실패했습니다. 원래 요청 패킷에 대한 콜백을 전달합니다.
						var resPacket = requestPacket.CreateResponse(ServerError.LogOut);
						CallResponse_(onResponse, resPacket);

						// Disconnect 처리합니다.
						OnDisconnect_(DisconnectReason.SessionTimeout, resPacket);
					}
				});
			}
			else
			{
				// 에러 처리 : 로그인 패킷이 등록되어 있지 않아서 바로 에러 처리
				var resPacket = requestPacket.CreateResponse(ServerError.LogOut);
				CallResponse_(onResponse, resPacket);

				// Disconnect 처리합니다.
				OnDisconnect_(DisconnectReason.SessionTimeout, resPacket);
			}
		}
		#endregion
	}
}
