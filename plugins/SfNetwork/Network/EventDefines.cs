using System;
namespace SF.Network
{
	public interface IEventReconnection
	{
		/// <summary>
		/// 재연결 준비 (
		/// </summary>
		event OnReconnect OnReconnect;
	}

	/// <summary>
	/// 서버 쓰래드를 실행시키는 인터페이스
	/// </summary>
	public interface IServerThread
	{
		bool Run();
	}

	/// <summary>
	/// 서버로부터 연결이 종료되었을 때 발생하는 이벤트 처리
	/// </summary>
	public delegate void OnDisconnect(DisconnectReason reason, ResponsePacket responsePatcket);

	/// <summary>
	/// 응답 받은 로그인 패킷으로 재연결 준비합니다.
	/// </summary>
	public delegate void OnReconnect(ResponsePacket responsePatcket);

	/// <summary>
	/// 재접속 이벤트
	/// </summary>
	public delegate void OnRelogin(LogoutReason reason, ResponsePacket responsePatcket);

	/// <summary>
	/// 패치 단계로 이동하라는 이벤트
	/// </summary>
	public delegate void OnPatch(PatchReason reason, ResponsePacket responsePacket);

	/// <summary>
	/// 강제로 서버와 데이터 동기화를 진행합니다.
	/// </summary>
	public delegate void SyncServerData(SyncCallback callback);
	public delegate void SyncCallback(bool succes);

	/// <summary>
	/// 재전송 여부
	/// </summary>
	public delegate void QueryRetransmit(RequestPacket reqPacket, AnswerRetransmit answer);
	public delegate void AnswerRetransmit(bool isRetransmit);

	/// <summary>
	/// 연결이 끊어진 이유
	/// </summary>
	public enum DisconnectReason
	{
		ClosedServer,       // 서버 점검중
		InvalidVersion,     // 버전이 맞지 않습니다
		Unauthorized,       // 인증에 실패하였습니다.
		SessionTimeout,     // 서버에서 로그아웃 되었습니다. (세션 타임 아웃)
		OtherLogin,         // 다른 디바이스에서 로그인되어 접속 종료
		Blocked,            // 차단 된 계정 (블락 처리)
		Freezing,           // 잠시 동안 접속 불가 (KickOff 상태)
		Unknown
	}

	/// <summary>
	/// 로그아웃이 된 이유 (다시 로그인하는 이벤트에 전달)
	/// </summary>
	public enum LogoutReason
	{
		SessionTimeout,
		OtherLogin,
		Unknown
	}

	public enum PatchReason
	{
		GameTablePatch,     // 첫 화면으로 이동해서 게임 테이블만 다시 받으면 됩니다.
		AppPatch            // App 다운로드 페이지로 안내합니다.
	}

	/// <summary>
	/// 패킷의 전송 상태를 전달합니다.
	/// </summary>
	public delegate void OnChangeIdleState(ConnectingState state, RequestPacket reqPacket);

	/// <summary>
	/// 에러가 발생했을때 발생하는 이벤트
	/// </summary>
	public delegate void OnError(ServerError error, ResponsePacket resPacket);

	/// <summary>
	/// 패킷 전송 상태
	/// </summary>
	public enum ConnectingState
	{
		ShowConnecting,   // 요청 패킷 전송 : 커넥팅 표시
		HideConnecting    // 응답 패킷 수신 : 케넥팅 감추기
	}
}
