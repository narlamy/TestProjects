using System;
using System.Collections.Generic;
using UnityEngine;

namespace N2.Network
{
    /// <summary>
    /// User reserves calling callback function on main-thread
    /// </summary>
	public class CallbackAgentObject : MonoBehaviour, ICallbackAgent
	{
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

		private Queue<ICallbackExecuter> mUpdateQueue = new Queue<ICallbackExecuter>();

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

			while (mUpdateQueue.Count > 0)
			{
				var callback = mUpdateQueue.Dequeue();
				callback.Execute();
			}
		}

		private Queue<ICallbackExecuter> mWaitingQueue = new Queue<ICallbackExecuter>();
		public void Call(ICallbackExecuter callback)
		{
			lock (mWaitingQueue)
			{
				mWaitingQueue.Enqueue(callback);
			}
		}
	}
}
