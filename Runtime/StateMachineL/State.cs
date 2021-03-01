namespace KoheiUtils
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
        /// OnEnter 内ではステートの遷移は許されない.
        /// </summary>
        public void OnEnter()
        {
            enterTime = Time.time;
            OnEnterInner();
        }

        public virtual void OnUpdate()
        {
        }

        public void OnExit()
        {
            previousExitTime = Time.time;
            OnExitInner();
        }

        protected virtual void OnEnterInner()
        {
        }

        protected virtual void OnExitInner()
        {
        }
    }
}