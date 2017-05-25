using UnityEngine;
using System;
using System.Collections.Generic;

namespace SF.Network
{    
    public static class CallbackAgentUtil
    {
        /// <summary>
        /// 가장 일반적인 패킷 응답 콜백 처리
        /// </summary>
        public static void AddCallback(this ICallbackAgent agent, OnResponse onResponse, ResponsePacket resPacket)
        {
            agent.ReserveCallback(ExeResponse.New(onResponse, resPacket));
        }

        ///// <summary>
        ///// onResponse 호출 예약
        ///// </summary>
        //public static void AddCallback(this ICallbackAgent agent, QueryRetransmit retransmit, OnResponse onResponse)
        //{
        //    agent.AddCallback(new ExeGeneral(() => { }));
        //}

        /// <summary>
        /// 
        /// </summary>
        public static void AddCallback(this ICallbackAgent agent, QueryRetransmit retransmit, RequestPacket reqPacket, AnswerRetransmit onAnswer)
        {
            agent.ReserveCallback(new ExeGeneral(() => { retransmit(reqPacket, onAnswer); }));
        }

        /// <summary>
        /// 대기 아이콘을 보여줄지 감출지 상태를 등록된 콜백에 전달합니다. (빙글 빙글... 접속중)
        /// 대기 아이콘 이벤트
        /// </summary>
        public static void AddCallback(this ICallbackAgent agent, OnChangeIdleState onChangeState, ConnectingState state, RequestPacket reqPacket)
        {
            agent.ReserveCallback(ExeIdle.New(onChangeState, state, reqPacket));
        }

        /// <summary>
        /// 
        /// </summary>
        public static void AddCallback(this ICallbackAgent agent, OnError onError, ServerError error, ResponsePacket resPacket)
        {
            agent.ReserveCallback(new ExeGeneral(() => { onError(error, resPacket); }));
        }

        /// <summary>
        /// 
        /// </summary>
        public static void AddCallback(this ICallbackAgent agent, OnPatch onPatch, PatchReason reason, ResponsePacket resPacket)
        {
            agent.ReserveCallback(new ExeGeneral(() => { onPatch(reason, resPacket); }));
        }

        /// <summary>
        /// 
        /// </summary>
        public static void AddCallback(this ICallbackAgent agent, OnRelogin onError, LogoutReason reason, ResponsePacket resPacket)
        {
            agent.ReserveCallback(new ExeGeneral(() => { onError(reason, resPacket); }));
        }

        /// <summary>
        /// 
        /// </summary>
        public static void AddCallback(this ICallbackAgent agent, OnDisconnect onDisconnect, DisconnectReason reason, ResponsePacket resPacket)
        {
            agent.ReserveCallback(new ExeGeneral(() => { }));
        }

        #region 내부 코드
        internal class ExeResponse : ICallback
        {
            static Queue<ExeResponse> Pool = new Queue<ExeResponse>(100);

            OnResponse OnResponse;
            ResponsePacket ResPacket;

            static internal ExeResponse New(OnResponse onResponse, ResponsePacket resPacket)
            {
                if (Pool.Count == 0)
                    return new ExeResponse() { OnResponse = onResponse, ResPacket = resPacket };

                var p = Pool.Dequeue();
                p.OnResponse = onResponse;
                p.ResPacket = resPacket;
                return p;
            }

            public void Dispose()
            {
                this.OnResponse = null;
                this.ResPacket = null;

                Pool.Enqueue(this);
            }

            public void Execute()
            {
                OnResponse(ResPacket);
            }
        }

        /// <summary>
        /// 커넥팅 이벤트
        /// </summary>
        internal class ExeIdle : ICallback
        {
            static Queue<ExeIdle> Pool = new Queue<ExeIdle>();

            OnChangeIdleState onChangeState;
            ConnectingState state;
            RequestPacket reqPacket;

            static internal ExeIdle New(OnChangeIdleState onChangeState, ConnectingState state, RequestPacket reqPacket)
            {
                ExeIdle p = null;

                if (Pool.Count == 0) p = new ExeIdle();
                else p = Pool.Dequeue();

                p.onChangeState = onChangeState;
                p.state = state;
                p.reqPacket = reqPacket;
                return p;
            }

            public void Dispose()
            {
                this.onChangeState = null;
                this.reqPacket = null;

                Pool.Enqueue(this);
            }

            public void Execute()
            {
                onChangeState(state, reqPacket);
            }
        }

        internal class ExeGeneral : ICallback
        {
            Action mAction;

            public ExeGeneral(Action action)
            {
                mAction = action;
            }

            public void Dispose()
            {
                mAction = null;
            }

            public void Execute()
            {
                mAction();
            }
        }
        #endregion
    }
}
