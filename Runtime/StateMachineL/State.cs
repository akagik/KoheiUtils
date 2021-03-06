﻿namespace KoheiUtils
{
    using UnityEngine;

    /// <summary>
    /// ステートマシンで管理されるステート.
    /// ステートは一意な stateId を持ち、ステートマシンによって id も管理される.
    ///
    /// ステートはパラメータを持ち、遷移先のステートにパラメータを渡すことができる.
    /// </summary>
    public class State
    {
        public int stateId; // このステートマシン内で一意な ID.

        public float enterTime;        // この状態に入った時間
        public float previousExitTime; // 前回このステートを抜けた時間

        public float elapsedTimeFromExit  => Time.time - previousExitTime;
        public float elapsedTimeFromEnter => Time.time - enterTime;

        // 自分を管理するステートマシン.
        public StateMachine masterStateMachine;

        /// <summary>
        /// パラメータ初期化処理.
        /// パラメータ遷移直前はこのメソッドが呼び出されていることが保証される.
        /// </summary>
        public virtual void InitializeParams()
        {
        }

        /// <summary>
        /// ステートに入ったときに呼び出される.
        /// OnEnter 中にステート遷移した場合は、OnExit は呼び出されない点に注意.
        /// </summary>
        public void OnEnterFirst(object input)
        {
            enterTime = Time.time;
            OnEnter(input);
        }
        
        public virtual void OnUpdate()
        {
        }

        public void OnExitFirst()
        {
            previousExitTime = Time.time;
            OnExit();
        }

        protected virtual void OnEnter(object input)
        {
        }

        protected virtual void OnExit()
        {
        }
    }
}