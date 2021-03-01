namespace KoheiUtils
{
    using System.Collections.Generic;
    using UnityEngine;

    public class StateMachine : State
    {
        private Dictionary<int, State> id2State;

        // 初期ステート. currentState は常に null 以外の値を入れるために導入.
        public State entryState { get; private set; }

        private int   registeredMaxStateId = -1;
        public  State currentState;
        public  State previousState;
        private bool  callingExit;
        private bool  onExitChanging; // OnExit 内で遷移があったかどうか？

        public bool isStateChanging { get; private set; }

        public int currentStateId => currentState.stateId;

        public int sizeOfStates => id2State.Count;

        public StateMachine()
        {
            entryState           = new EmptyState();
            stateId              = -1;
            registeredMaxStateId = -1;
            id2State             = new Dictionary<int, State>();

            RegisterState(entryState);
            currentState  = entryState;
            previousState = entryState;
        }

        public int RegisterState(State newState)
        {
            registeredMaxStateId += 1;
            newState.stateId     =  registeredMaxStateId;
            newState.InitializeParams();
            newState.masterStateMachine = this;

            id2State.Add(registeredMaxStateId, newState);
            return registeredMaxStateId;
        }

        public int RegisterState(int stateId, State newState)
        {
            registeredMaxStateId = stateId;
            newState.stateId     = registeredMaxStateId;
            newState.InitializeParams();
            newState.masterStateMachine = this;

            if (id2State.ContainsKey(stateId))
            {
                return -1;
            }

            id2State.Add(registeredMaxStateId, newState);
            return registeredMaxStateId;
        }

        public void RemoveState(int stateId)
        {
            id2State.Remove(stateId);
        }

        public State GetState(int stateId)
        {
            return id2State[stateId];
        }

        public Dictionary<int, State>.KeyCollection Keys => id2State.Keys;

        public void ChangeStateWithoutParam(int newStateId)
        {
            if (id2State.TryGetValue(newStateId, out State newState))
            {
                ChangeStateWithoutParam(newState);
            }
            else
            {
                Debug.LogError("指定の stateId は見つかりません: " + newStateId);
            }
        }

        /// <summary>
        /// パラメータなし遷移
        /// </summary>
        public void ChangeStateWithoutParam(State next)
        {
//            Debug.Log($"ChangeState: {currentStateId}->{next.stateId}");

            // すでにステート遷移中の場合は OnExit をスキップする.
            if (isStateChanging)
            {
                // 基本的に遷移中に遷移が起こるのは、OnExit のタイミングだけだが、それ以外で
                // 遷移が発生した場合は例外として処理する.
                if (!callingExit)
                {
                    Debug.LogError("遷移中の不正な遷移が発生");
                    return;
                }

                onExitChanging = true;

                this.currentState.InitializeParams();
                this.currentState = next;
                this.currentState.OnEnter();
                return;
            }

            isStateChanging = true;

            previousState = currentState;

            callingExit = true;
            currentState.OnExit();
            callingExit = false;

            if (!onExitChanging)
            {
                this.currentState.InitializeParams();
                this.currentState = next;
                this.currentState.OnEnter();
            }

            onExitChanging  = false;
            isStateChanging = false;
        }

        /// <summary>
        /// 現在の遷移を抜けたときに、さらに遷移が発生した場合は true を返す.
        /// ChangeStateBegin メソッドは、ステート先にパラメータを渡すときに利用する.
        /// 流れとしては以下のような感じになる:
        ///
        /// 1. ChagneStateBegin 呼び出し
        /// ↓
        /// 2. ステート遷移先にパラメータをセット
        /// ↓
        /// 3. ChangeStateEnd 呼び出し
        /// 
        /// </summary>
        public bool ChangeStateBegin()
        {
//            Debug.Log($"ChangeStateBegin: From {currentState.stateId}");

            // すでにステート遷移中の場合は OnExit をスキップする.
            // 基本的に遷移中に遷移が起こるのは、OnExit のタイミングだけ.
            if (isStateChanging)
            {
                if (!callingExit)
                {
                    Debug.LogError("遷移中の不正な遷移が発生");
                    return false;
                }

                onExitChanging = true;
                this.currentState.InitializeParams();
                return true;
            }

            isStateChanging = true;
            previousState   = currentState;

            callingExit = true;
            currentState.OnExit();
            callingExit = false;

            if (onExitChanging)
            {
                onExitChanging  = false;
                isStateChanging = false;
                return false;
            }

            this.currentState.InitializeParams();
            return true;
        }

        public void ChangeStateEnd(State newState)
        {
//            Debug.Log($"ChangeStateEnd: To {newState.stateId}");

            this.currentState = newState;
            this.currentState.OnEnter();

            isStateChanging = false;
        }

        public override void OnUpdate()
        {
            currentState.OnUpdate();
        }

        protected override void OnEnterInner()
        {
            currentState.OnEnter();
        }

        protected override void OnExitInner()
        {
            currentState.OnExit();
        }
    }
}