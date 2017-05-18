using UnityEngine;
using System;
using System.Collections.Generic;

namespace N2.Network
{
    public interface ICallbackExecuter
    {
        void Execute();
    }

    public interface ICallbackAgent
    {
        void Call(ICallbackExecuter callback);
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
