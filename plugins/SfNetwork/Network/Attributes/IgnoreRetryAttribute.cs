using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SF.Network
{
    /// <summary>
    /// Request 패킷 전송 실패시 재시도를 하지 않습니다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class IgnoreRetryAttribute : Attribute
    {
        public IgnoreRetryAttribute()
        {

        }
    }
    
    internal static class IgnoreRetryHelper
    {
        /// <summary>
        /// 패킷 전송 실패시 패킷 재시도를 무시합니까?
        /// </summary>
        public static bool IsIgnoreRetry(this RequestPacket reqPacket)
        {
            if (reqPacket == null)
                return false;

            if (reqPacket.IsIgnoreRetry == null)
            {
                var attr = Attribute.GetCustomAttribute(reqPacket.GetType(), typeof(IgnoreRetryAttribute)) as IgnoreRetryAttribute;
                reqPacket.IsIgnoreRetry = attr != null;
            }
            return reqPacket.IsIgnoreRetry.Value;
        }
    }
}
