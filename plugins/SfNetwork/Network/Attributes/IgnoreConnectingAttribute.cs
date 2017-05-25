using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SF.Network
{
    /// <summary>
    /// RequestPacket을 전송하더라도 Connecting 이벤트를 발생시키지 않습니다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class IgnoreConnectingAttribute : Attribute
    {
        public IgnoreConnectingAttribute()
        {
        }

        /// <summary>
        /// ignore connecting 여부를 동적으로 결정 짓도록 할 수 있습니다.
        /// </summary>
        public bool IsIgnore(RequestPacket reqPacket)
        {
            var query = reqPacket as IQueryIgnoreConnecting;
            return query != null ? query.IsIgnore : true;
        }
    }

    /// <summary>
    /// ignore connecting 여부를 동적으로 결정 짓도록 구현합니다. 
    /// 이를 사용하기 위해서는 RequestPacket에 IgnoreConnecting 속성이 걸려 있어야 합니다.
    /// </summary>
    public interface IQueryIgnoreConnecting
    {
        bool IsIgnore { get; set; }
    }

    internal static class IgnoreConnectingHelper
    {
        /// <summary>
        /// 커넥팅 상태를 표시하진 않는지 물어봅니다.
        /// </summary>
        public static bool IsIgnoreConnecting(this RequestPacket reqPacket)
        {
            if (reqPacket == null)
                return false;

            if (reqPacket.IsIgnoreConnecting == null)
            {
                var attr = Attribute.GetCustomAttribute(reqPacket.GetType(), typeof(IgnoreConnectingAttribute)) as IgnoreConnectingAttribute;
                reqPacket.IsIgnoreConnecting = (attr != null) ? attr.IsIgnore(reqPacket) : false;
            }

            return reqPacket.IsIgnoreConnecting.Value;
        }
    }
}
