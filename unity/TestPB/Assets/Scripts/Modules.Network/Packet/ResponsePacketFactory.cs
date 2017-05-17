using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cocoonbeat.Net
{
    /// <summary>
    /// RequestPacket의 ResposneType을 가지고 ResopnsePacket 생성
    /// </summary>
    internal static class ResponsePacketFactory
    {
        /// <summary>
        /// 아무것도 설정하지 않은 ResponsePacket 객체를 생성합니다.
        /// </summary>
        internal static ResponsePacket CreateResponse(this RequestPacket req)
        {
            return CreateResponse(req, 0, null);
        }

        /// <summary>
        /// 에러 코드를 강제로 설정해서 ResponsePacket 객체를 생성합니다.
        /// </summary>
        internal static ResponsePacket CreateResponse(this RequestPacket req, int errCode, string errMsg = null)
        {
            // [2016-07-11] narlamy
            // 구현 클래스에 파라메터 3개를 입력받을 수 있는 생성자가 정의되어 있지 않다면
            // 올바르게 인스턴스가 생성되지 못 할 수도 있습니다.
            // iOS에서 동작 잘 되지 않음

            var resPacket = (req != null) ? System.Activator.CreateInstance(req.ResponseType) as ResponsePacket : null;
            if (resPacket != null)
            {
                resPacket.SetRequestPacket(req);
                resPacket.SetErrorCode(errCode, errMsg);
                return resPacket;
            }
            else
            {
                // [2016-01-30] narlamy
                // 하지만 이 패킷이 만들어질 가능성은 거의 없습니다.
                return new Default.ErrorResponse(req, errMsg);
            }
        }
        
        /// <summary>
        /// 입력받은 문자열을 해석하여 적절한 패킷 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="responseText">패킷을 통해 넘어받은 내용</param>
        internal static ResponsePacket CreateResponse(this RequestPacket request, string responseText)
        {
            return PacketFactory.Instance.CreateResponse(request, responseText);
        }
    }

    /// <summary>
    /// 응답 패킷을 넘겨 받는 이벤트 용
    /// </summary>
    public delegate void OnResponse(ResponsePacket response);    
}
