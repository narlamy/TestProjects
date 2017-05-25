﻿
namespace SF.Network
{
    /// <summary>
    /// 네트워크 모듈에서 사용하는 로그, 콜백 등의 객체 생성
    /// </summary>
    internal static class ModuleFactory
    {
        internal static Dev.ILogger CreateLogger()
        {
            return new UnityLogger();
        }

        internal static ICallbackAgent CreateCallbackAgent()
        {
            return CallbackAgentObject.Create();
        }

        internal static IPacketTable CreatePacketTable()
        {
            return new UnityPacketTable();
        }
    }
}
