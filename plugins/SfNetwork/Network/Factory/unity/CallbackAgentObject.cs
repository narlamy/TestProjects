using System;
using System.Collections.Generic;
using UnityEngine;

namespace SF.Network
{
    /// <summary>
    /// User reserves calling callback function on main-thread
    /// </summary>
	public class CallbackAgentObject : MonoBehaviour, ICallbackAgent
	{
        /// <summary>
        /// 예약 된 콜백을 Update() 시에 처리할 수 있는 최대 개수
        /// 큐에 너무 많은 콜백이 등록되었을 경우 예방 코드
        /// </summary>
        const int MAX_DEQUEUE_COUNT = 100;

        private static CallbackAgentObject mInstance = null;
		internal static ICallbackAgent Create()
		{
			if (object.ReferenceEquals(mInstance, null))
			{
				var go = new GameObject("$CallbackAgent");
				go.hideFlags = HideFlags.DontUnloadUnusedAsset;

                mInstance = go.AddComponent<CallbackAgentObject>();
			}
            return mInstance;
		}

		private Queue<ICallback> mUpdateQueue = new Queue<ICallback>();

        void OnApplicationQuit()
        {
            if(mOnQuitApp!=null)
                mOnQuitApp();
        }

        Action mOnQuitApp = null;
        public event Action OnQuitApp { add { mOnQuitApp += value; } remove { mOnQuitApp -= value; } }

        void Update()
		{
			// 에디
			_UpdateCallbacks();
		}

        void _UpdateCallbacks()
		{
			// step1 : into update queue from waiting queue
			lock (mWaitingQueue)
			{
				while (mWaitingQueue.Count > 0)
				{
					var callback = mWaitingQueue.Dequeue();
					mUpdateQueue.Enqueue(callback);
				}
			}

            int dequeCount = MAX_DEQUEUE_COUNT;

            while (mUpdateQueue.Count > 0 && dequeCount-- > 0)
			{
                using (var callback = mUpdateQueue.Dequeue())
                {
                    callback.Execute();
                }
			}
		}

		private Queue<ICallback> mWaitingQueue = new Queue<ICallback>();
		public void ReserveCallback(ICallback callback)
		{
			lock (mWaitingQueue)
			{
				mWaitingQueue.Enqueue(callback);
			}
		}
	}
}
