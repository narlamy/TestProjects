using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SF.Network
{
    public sealed class ResponseBinary : ResponsePacket
    {
        public ResponseBinary()
        {
        }

        string mResName = null;
        public ResponseBinary(RequestBinary request, byte[] bytes)
        {
            mResName = request.PacketName.Replace("Req", "Res");
            SetRequestPacket(request);
            SetBytes(bytes);
        }

        internal override ServerError GetError()
        {
            //return ServerError.Create<>();
            return new ServerError();
        }

        public override string PacketName
        {
            get { return mResName != null ? mResName : base.PacketName; }
        }

        private byte[] mBytes;

        internal void SetBytes(byte[] bytes)
        {
            mBytes = bytes;
        }

        public byte[] GetBytes()
        {
            return mBytes;
        }
    }
}
