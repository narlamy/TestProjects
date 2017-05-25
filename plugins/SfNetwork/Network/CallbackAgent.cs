using UnityEngine;
using System;
using System.Collections.Generic;

namespace SF.Network
{
    public interface ICallback : IDisposable
    {
        void Execute();
    }

    public interface ICallbackAgent
    {
        /// <summary>
        /// 콜백 예약
        /// </summary>
        void ReserveCallback(ICallback callback);
        
        /// <summary>
        /// 어플리케이션 종료시 호출
        /// </summary>
        event Action OnQuitApp;
    }
    
    public static class CallbackAgent
    {
        private static ICallbackAgent mInstance;
        public static ICallbackAgent Instance { get { return mInstance; } }

        static CallbackAgent()
        {
            mInstance = ModuleFactory.CreateCallbackAgent();
        }
    }
}
