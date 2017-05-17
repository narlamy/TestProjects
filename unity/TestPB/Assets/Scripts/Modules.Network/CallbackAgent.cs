using UnityEngine;
using System;
using System.Collections.Generic;

namespace Net.Network
{
    public interface ICallbackExecuter
    {
        void Execute();
    }

    public class CallbackAgent : MonoBehaviour
    {
        private static CallbackAgent mInstance = null;
        static public CallbackAgent Instance
        {
            get 
            {
                if(object.ReferenceEquals(mInstance, null))
                {
                    var go = new GameObject("$CallbackAgent");
                    mInstance = go.AddComponent<CallbackAgent>();

                    go.hideFlags = HideFlags.DontUnloadUnusedAsset;
                }
                return mInstance; 
            }
        }

		private Queue<ICallbackExecuter> mUpdateQueue = new Queue<ICallbackExecuter>();

        void Update()
        {
            // 에디
            _UpdateCallbacks();
        }

        void _UpdateCallbacks()
        {
            // step1 : into update queue from waiting queue
            lock(mWaitingQueue)
            {
                while(mWaitingQueue.Count > 0)
                {
                    var callback = mWaitingQueue.Dequeue();
                    mUpdateQueue.Enqueue(callback);
                }
            }

            while(mUpdateQueue.Count > 0)
            {
                var callback = mUpdateQueue.Dequeue();
                callback.Execute();
            }
        }

		private Queue<ICallbackExecuter> mWaitingQueue = new Queue<ICallbackExecuter>();
		public void Add(ICallbackExecuter callback)
        {
            lock (mWaitingQueue)
            {
                mWaitingQueue.Enqueue(callback);
            }
        }
    }
}
