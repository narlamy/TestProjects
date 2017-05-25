using System;
using System.Reflection;
using LitJson;

namespace SF.Network
{
    /// <summary>
    /// 모든 패킷에러에서 공용으로 사용하는 에러
    /// </summary>
    public class UnionError : ServerError
    {
    }

    /// <summary>
    /// 아래 에러 코드는 모든 에러에서 동일한 값으로 정의됩니다. (서버 코드는 UnionError로 정의)
    /// </summary>
    public class ServerError : IServerError, System.IEquatable<ServerError>
    {
        // [2016-01-08] narlamy
        // abstract class로 만들고 싶은데 시리얼라이징 오류가 있어서 못했던거 같음
        // 확실한 기억은 아니지만, 오동작이 일어나는지 확인후 수정 필요!!!

        public const short None = 0;                   // 성공

        public const short Unknown = -100;             // 알 수 없는 에러
        public const short NetworkError = -101;        // 네트웍 장애로 인해 올바른 패킷 생성 실패
        public const short ClosedServer = -102;        // 서버 점검중
        public const short InvalidVersion = -103;      // 버전이 일치하지 않습니다.
        public const short RequirePatch = -104;        // 패치가 필요합니다.
        public const short PermissionError = -105;     // 권한이 없습니다.
        public const short Unauthorized = -106;        // 로그인을 필요합니다.
        public const short OtherLogin = -107;          // 다른 곳에서 로그인되었습니다.
        public const short LogOut = -108;              // 세션이 만료되어 로그 아웃되었습니다.
        public const short NotFoundRequest = -109;     // 요청한 패킷이 서버에 정의되어 있지 않습니다.
        public const short ResponseError = -110;       // 응답 처리중에 오류 발생 (서버가 뻗어있을지도 몰라요)
        public const short Blocked = -111;             // 블락당한 계정
        public const short Freezing = -112;            // 잠시 접속을 중단시킵니다. (GM이 유저 정보 수정 중, KickOff 상태)

        public const short InvalidUser = -200;         // 잘못된 유저 정보
        public const short InternalError = -201;       // 서버 내부 에러
        public const short Reject = -202;              // 조건이 미흡하여 서버에서 처리 거부
        public const short NotFound = -203;            // 해당 정보로 데이터에 접근할 수 없을때 발생 (아이템, 피규어 검색 실패)
        public const short DecryptionFailed = -204;    // 암호화 해제 실패
        public const short InvalidRequest = -205;      // 잘못된 요청
        public const short InvalidParam = -206;        // 잘못된 파라메터
        public const short Deprecated = -207;          // 더 이상 사용하지 않습니다.
        public const short RequireSyncUser = -208;     // 유저 데이터 동기화가 필요합니다.

        public const short Hacked = -300;              // 해킹 패킷으로 의심됨
        public const short ParsingError = -301;        // 패킷 파싱 오류
        public const short NotDefined = -302;          // 정의되지 않은 패킷
        public const short CheckFailed = -303;         // 검사 실패
        public const short Disabled = -304;            // 네트웍 모듈이 꺼져있습니다.
        public const short StreamError = -305;         // Stream 에러 (네트웍 오류가 발생하여 전달 받을 수 있는 스트림이 없는 경우)
        public const short BokenPacket = -306;         // 잘못된 패킷
        public const short NullInstance = -307;        // 인스턴스가 유효하지 않습니다.

        /// <summary>
        /// 패킷마다 고유한 아이디
        /// </summary>
        [JsonIgnore]
        public int PacketUniqueNum { get; set; }

        [JsonIgnore]
        public int ErrorCode { get; set; }

        /// <summary>
        /// PacketUniqueNum와 ErrorCode를 합쳐서 반환 (패킷마다 고유의 패킷 아이디를 가집니다.)
        /// 이 값은 반드시 양수입니다.
        /// 본 값을 가지고 어떤 패킷의 어떤 에러인지 파악이 가능합니다.
        /// </summary>
        public int FullErrorCode { get { return PacketUniqueNum + Math.Abs(ErrorCode); } }

        /// <summary>
        /// 에러 코드를 가지고 에러 객체를 생성합니다.
        /// </summary>
        public static T Create<T>(int erroCode) where T : ServerError
        {
            var inst = System.Activator.CreateInstance<T>();
            inst.ErrorCode = erroCode;
            return inst;
        }

        /// <summary>
        /// IServerError에서 값을 복사해옵니다.
        /// </summary>
        public void CopyFrom(IServerError error)
        {
            this.PacketUniqueNum = error.PacketUniqueNum;
            this.ErrorCode = error.ErrorCode;
        }

        /// <summary>
        ///  입력 받은 에러 값이 성공인지 아닌지 반환
        /// </summary>
        public static bool IsSuccess(int errCode)
        {
            return errCode == (int)None;
        }

        public static bool operator ==(ServerError errObj, int errCode)
        {
            if (errObj == null)
                return false;

            return errObj.ErrorCode == errCode;
        }

        public static bool operator !=(ServerError errObj, int errCode)
        {
            if (errObj == null)
                return false;

            return errObj.ErrorCode != errCode;
        }

        public static bool operator ==(int errCode, ServerError errObj)
        {
            if (errObj == null)
                return false;

            return errObj.ErrorCode == errCode;
        }

        public static bool operator !=(int errCode, ServerError errObj)
        {
            if (errObj == null)
                return false;

            return errObj.ErrorCode != errCode;
        }

        public static bool operator ==(ServerError a, ServerError b)
        {
            if (object.ReferenceEquals(a, b))
                return true;

            if (object.ReferenceEquals(a, null))
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(ServerError a, ServerError b)
        {
            if (object.ReferenceEquals(a, b))
                return false;

            if (object.ReferenceEquals(a, null))
                return false;

            return !a.Equals(b);
        }

        public bool Equals(ServerError obj)
        {
            if (object.ReferenceEquals(obj, null))
                return false;

            return ErrorCode == obj.ErrorCode;
        }

        public override bool Equals(object obj)
        {
            if (obj is ServerError)
                return Equals((ServerError)obj);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            //return ErrorID << 12 | ErrorCode;
            return FullErrorCode;
        }

        public string GetErrorName()
        {
            return GetType().Name + "_" + ToString();
        }

        public override string ToString()
        {
            short errCode = (short)ErrorCode;

            try
            {
                foreach (var fi in GetType().GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy))
                {
                    var val = fi.GetValue(this);

                    if (short.Equals(errCode, val))
                        return fi.Name;
                }
            }
            catch (Exception e)
            {
                Dev.Logger.LogError(e.ToString());
            }
            return string.Format("\"{0}\"", errCode);
        }
    }
}

namespace SF
{ 
    public static class ServerErrorChecker
    {
        public static bool IsNull(this Network.ServerError err)
        {
            return ReferenceEquals(err, null);
        }
    }
}
