using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cocoonbeat.Net
{
    /// <summary>
    /// 에러가 발생하더라도 본 속성이 붙어 있는 ResponsePacket은
    /// WebServer의 에러 이벤트를 발생시키지 않습니다.
    /// 에러 코드가 사라진다는 의미는 아닙니다.
    /// </summary>
    public class IgnoreErrorEventAttribute : Attribute
    {
        public IgnoreErrorEventAttribute()
        {
            // ResponsePacket에 붙입니다.
        }
    }

    internal static class IngoreErrorEventHelper
    {
        /// <summary>
        /// 에러 이벤트 처리하지 않는 패킷인지 확인
        /// </summary>
        public static bool IsIgnoreErrorEvent(this ResponsePacket resPacket)
        {
            if (resPacket == null)
                return false;

            var attr = Attribute.GetCustomAttribute(resPacket.GetType(), typeof(IgnoreErrorEventAttribute));
            return attr != null;
        }
    }
}
