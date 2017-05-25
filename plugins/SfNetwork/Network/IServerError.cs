using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SF.Network
{
    public interface IServerError
    {
        /// <summary>
        /// 패킷의 고유 번호
        /// </summary>
        int PacketUniqueNum { get; }

        /// <summary>
        /// 서버에서 보내온 에러 코드
        /// </summary>
        int ErrorCode { get; }

        /// <summary>
        /// ErrorID와 ErrorCode를 합친값
        /// </summary>
        int FullErrorCode { get; }
    };

    public static class ServerErrorHelper
    {
        /// <summary>
        /// ServerError와 연결된 패킷 이름을 반환합니다.
        /// </summary>
        public static string GetPacketName(this IServerError error)
        {
            if (error == null)
                return "";

            return PacketTable.Instance.FindPacketName(error.PacketUniqueNum);
        }

        /// <summary>
        /// 성공 여부 반환
        /// </summary>
        public static bool IsSuccess(this IServerError error)
        {
            return error.ErrorCode == ServerError.None;
        }
    }
}
