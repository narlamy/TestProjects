using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace N2.Network.Default
{
    /// <summary>
    /// 에러 내용을 담고 있는 에러 패킷
    /// </summary>
    internal class ErrorResponse : ResponsePacket
    {
        private RequestPacket mReqeust;
        private string mContent;

        public ErrorResponse(RequestPacket request, string content)
        {
            mReqeust = request;
            mContent = content!=null ? content : "";
        }

        public RequestPacket RequestPacket { get { return mReqeust; } }
        public string Content { get { return mContent; } }

        public override string ToString()
        {
            return mContent;
        }
        internal override ServerError GetError()
        {
            return new ServerError() { ErrorCode = ServerError.Unknown };
        }

        public override bool IsSuccess
        {
            get { return false; }
        }
    }
}
