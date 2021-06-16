namespace KoheiUtils
{
    using System.Collections.Generic;
    using UnityEngine;

    public class StateMachine : State
    {
        public State currentState  { get; protected set; }
        public State previousState { get; protected set; }

        // 初期ステート. currentState は常に null 以外の値を入れるために導入.
        public State entryState { get; private set; }

        public bool duringTransition { get; private set; }

        private Dictionary<int, State> id2State;

        private int  registeredMaxStateId = -1;
        private bool innerTransition; // OnExit 内で遷移があったかどうか？

        // ----------------------------------------------
        // Properties
        // ----------------------------------------------

        public Dictionary<int, State>.KeyCollection Keys => id2State.Keys;
    
        public int currentStateId => currentState.stateId;
        public int sizeOfStates   => id2State.Count;


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
            registeredMaxStateId        += 1;
            newState.stateId            =  registeredMaxStateId;
            newState.masterStateMachine =  this;

            id2State.Add(registeredMaxStateId, newState);
            return registeredMaxStateId;
        }

        public int RegisterState(int stateId, State newState)
        {
            registeredMaxStateId        = stateId;
            newState.stateId            = registeredMaxStateId;
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

        public void ClearState()
        {
            id2State.Clear();
        }

        public State GetState(int stateId)
        {
            return id2State[stateId];
        }

        public T GetState<T>(int stateId) where T : State
        {
            return id2State[stateId] as T;
        }

        public void Transition(int newStateId)
        {
            if (id2State.TryGetValue(newStateId, out State newState))
            {
                Transition(newState);
            }
            else
            {
                Debug.LogError("指定の stateId は見つかりません: " + newStateId);
            }
        }

        /// <summary>
        /// パラメータなし遷移
        /// </summary>
        public void Transition(State next, object input = null)
        {
            if (verbose)
            {
                Debug.Log($"Transition: {currentStateId}->{next.stateId} (input: {input})");
            }

            // すでにステート遷移中の場合は OnExit をスキップする.
            if (duringTransition)
            {
                innerTransition = true;

                next.InitializeParams();
                next.OnEnterFirst(input);
                this.currentState = next;

                return;
            }

            duringTransition = true;
            previousState    = currentState;

            currentState.OnExitFirst();

            if (!innerTransition)
            {
                next.InitializeParams();
                next.OnEnterFirst(input);

                currentState = next;
            }

            innerTransition  = false;
            duringTransition = false;
        }

        public override void OnUpdate()
        {
            currentState.OnUpdate();
        }

        protected override void OnEnter(object input)
        {
            currentState.OnEnterFirst(input);
        }

        protected override void OnExit()
        {
            currentState.OnExitFirst();
        }

        // ----------------------------------------------
        // For Debug
        // ----------------------------------------------
        public bool verbose;
    }
}