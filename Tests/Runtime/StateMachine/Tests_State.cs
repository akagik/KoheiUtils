using System;
using KoheiUtils;
using NUnit.Framework;

namespace KoheiUtils.Tests
{
    using NUnit.Framework;
    using KoheiUtils;

    public class Tests_State
    {
        [Test]
        public void Test_InitialStateMachine()
        {
            StateMachine fsm = new StateMachine();

            // 初期ではエントリ状態のみ
            Assert.AreEqual(fsm.sizeOfStates, 1);

            // 初期状態は null ではない.
            Assert.AreNotEqual(null, fsm.currentState);
            Assert.AreEqual(fsm.entryState, fsm.currentState);
            Assert.AreEqual(fsm.entryState, fsm.previousState);
            Assert.AreEqual(0, fsm.currentState.stateId);
        }

        [Test]
        public void Test_RegisterState()
        {
            StateMachine fsm   = new StateMachine();
            State        state = new State();

            // ステートの追加
            int stateId = fsm.RegisterState(state);

            // ステートID は連番でつけられる.
            Assert.AreEqual(1, stateId);
            Assert.AreEqual(2, fsm.sizeOfStates);
            Assert.AreEqual(fsm.entryState, fsm.currentState);
        }

        [Test]
        public void Test_ChangeStateWithoutParam()
        {
            StateMachine fsm   = new StateMachine();
            State        state = new State();

            // ステートの追加
            int stateId = fsm.RegisterState(state);

            fsm.Transition(state);
            Assert.AreEqual(state, fsm.currentState);
        }

        [Test]
        public void Test_TransitionWithParams()
        {
            int          order = 0;
            StateMachine fsm   = new StateMachine();
            A            a     = new A();
            B            b     = new B();

            fsm.RegisterState(a);
            fsm.RegisterState(b);

            fsm.Transition(a);
            
            // A -> B(34)
            b.OnEnterFunc = (input) =>
            {
                // 
                Assert.AreEqual(34, (int) input);
            };

            fsm.Transition(b, 34);
            
            // B -> B(21)
            b.OnEnterFunc = (input) =>
            {
                // 
                Assert.AreEqual(21, (int) input);
            };

            fsm.Transition(b, 21);
            
            // B -> B(null)
            b.OnEnterFunc = (input) =>
            {
                // 
                Assert.AreEqual(100, (int) b.value);
            };

            fsm.Transition(b);
        }

        // イベントメソッドの呼び出し順
        [Test]
        public void Test_EventFuncCallOrder()
        {
            int          order  = 0;
            StateMachine fsm    = new StateMachine();
            A            stateA = new A();
            B            stateB = new B();

            fsm.RegisterState(stateA);
            fsm.RegisterState(stateB);

            stateA.InitializeParamsFunc = () =>
            {
                Assert.AreEqual(0, order);
                order++;
            };

            stateA.OnEnterFunc = (input) =>
            {
                Assert.AreEqual(1, order);
                order++;
            };

            stateA.OnExitFunc = () =>
            {
                Assert.AreEqual(2, order);
                order++;
            };

            stateB.InitializeParamsFunc = () =>
            {
                Assert.AreEqual(3, order);
                order++;
            };

            stateB.OnEnterFunc = (input) =>
            {
                Assert.AreEqual(4, order);
                order++;
            };

            fsm.Transition(stateA);
            Assert.AreEqual(stateA, fsm.currentState);
            fsm.Transition(stateB);
            Assert.AreEqual(stateB, fsm.currentState);
            Assert.AreEqual(stateA, fsm.previousState);
        }

        // Exit 中でのステート遷移
        [Test]
        public void Test_ChangeStateOnExit()
        {
            int          order  = 0;
            StateMachine fsm    = new StateMachine();
            A            stateA = new A();
            B            b1     = new B();
            B            b2     = new B();

            fsm.RegisterState(stateA);
            fsm.RegisterState(b1);
            fsm.RegisterState(b2);

            fsm.Transition(stateA);

            stateA.OnExitFunc = () =>
            {
                Assert.AreEqual(0, order);
                order++;

                // A から抜けるときに entry に移行するようにする.
                fsm.Transition(b2);
            };

            b1.OnEnterFunc = (input) =>
            {
                // b1 には移行しない.
                Assert.Fail();
            };

            b2.InitializeParamsFunc = () =>
            {
                Assert.AreEqual(1, order);
                order++;
            };

            b2.OnEnterFunc = (input) =>
            {
                // b1 の代わりに b2 に移行する
                Assert.AreEqual(2, order);
                order++;
            };

            Assert.AreEqual(stateA, fsm.currentState);
            fsm.Transition(b1);
            Assert.AreEqual(b2, fsm.currentState);
            Assert.AreEqual(stateA, fsm.previousState);
        }

        // イベントメソッドの呼び出し順
        [Test]
        public void Test_ChangeStateWithParams()
        {
            int          order = 0;
            StateMachine fsm   = new StateMachine();
            A            a     = new A();
            B            b     = new B();

            fsm.RegisterState(a);
            fsm.RegisterState(b);

            fsm.Transition(a);

            a.OnExitFunc = () =>
            {
                Assert.AreEqual(0, order);
                order++;
            };

            b.InitializeParamsFunc = () =>
            {
                Assert.AreEqual(1, order);
                order++;
            };

            b.OnEnterFunc = (input) =>
            {
                Assert.AreEqual(2, order);
                Assert.AreEqual(5, b.value);
                order++;
            };

            Assert.AreEqual(a, fsm.currentState);

            fsm.Transition(b, 5);
            Assert.AreEqual(5, b.value);

            Assert.AreEqual(b, fsm.currentState);
            Assert.AreEqual(a, fsm.previousState);

            Assert.AreEqual(3, order);
        }

        // パラメータあり遷移の Exit にて 遷移発生
        [Test]
        public void Test_ChangeStateWithParamsOnExit()
        {
            int          order = 0;
            StateMachine fsm   = new StateMachine();
            A            a     = new A();
            B            b1    = new B();
            B            b2    = new B();

            fsm.RegisterState(a);
            fsm.RegisterState(b1);
            fsm.RegisterState(b2);

            fsm.Transition(a);

            a.OnExitFunc = () =>
            {
                Assert.AreEqual(0, order);
                order++;

                fsm.Transition(b2);
            };

            b1.OnEnterFunc = (input) => { Assert.Fail(); };

            b2.InitializeParamsFunc = () =>
            {
                Assert.AreEqual(1, order);
                order++;
            };


            b2.OnEnterFunc = (input) =>
            {
                Assert.AreEqual(2, order);
                order++;
            };

            Assert.AreEqual(a, fsm.currentState);

            // a -> b1 に遷移
            fsm.Transition(b1);

            Assert.AreEqual(3, order);
            Assert.AreEqual(b2, fsm.currentState);
            Assert.AreEqual(a, fsm.previousState);
        }

        // パラメータあり遷移の Exit にて パラメータあり遷移発生
        [Test]
        public void Test_ChangeStateWithParamsOnExitParams()
        {
            int          order = 0;
            StateMachine fsm   = new StateMachine();
            A            a     = new A();
            B            b1    = new B();
            B            b2    = new B();

            fsm.RegisterState(a);
            fsm.RegisterState(b1);
            fsm.RegisterState(b2);


            a.InitializeParamsFunc = () =>
            {
                Assert.AreEqual(0, order);
                order++;
            };

            a.OnExitFunc = () =>
            {
                Assert.AreEqual(1, order);
                order++;

                Assert.IsTrue(fsm.duringTransition);

                order++;
                fsm.Transition(b2, 15);
                order++;
            };

            b1.OnEnterFunc = (input) => { Assert.Fail(); };

            b2.OnEnterFunc = (input) =>
            {
                Assert.AreEqual(3, order);
                order++;

                Assert.AreEqual(15, b2.value);
            };

            fsm.Transition(a);

            Assert.AreEqual(a, fsm.currentState);

            fsm.Transition(b1, 5);
            Assert.AreEqual(5, order);
            order++;
            Assert.AreEqual(b2, fsm.currentState);
            Assert.AreEqual(a, fsm.previousState);
        }

        // パラメータ初期化チェック
        [Test]
        public void Test_InitializeParams()
        {
            StateMachine fsm = new StateMachine();
            A            a   = new A();
            B            b1  = new B();
            B            b2  = new B();

            fsm.RegisterState(a);
            fsm.RegisterState(b1);
            fsm.RegisterState(b2);

            fsm.Transition(a);

            Assert.AreEqual(a, fsm.currentState);

            // A -> B1 (5)
            fsm.Transition(b1, 5);

            Assert.AreEqual(5, b1.value);

            // B1 -> B2 (20)
            fsm.Transition(b2, 20);

            Assert.AreEqual(20, b2.value);

            // B2 -> A
            fsm.Transition(a);
        }

        // 自己遷移
        [Test]
        public void Test_ChangeToSelf()
        {
            StateMachine fsm = new StateMachine();
            A            a   = new A();

            fsm.RegisterState(a);

            // Entry -> A
            fsm.Transition(a);
            Assert.AreEqual(a, fsm.currentState);

            // A -> A
            fsm.Transition(a);
            Assert.AreEqual(a, fsm.previousState);
            Assert.AreEqual(a, fsm.currentState);
        }

        // パラメータ付き自己遷移
        [Test]
        public void Test_ChangeToSelfWithParams()
        {
            StateMachine fsm = new StateMachine();
            B            b   = new B();
            fsm.RegisterState(b);

            // Entry -> B
            fsm.Transition(b);

            // パラメータを渡さない場合は初期状態になる.
            Assert.AreEqual(100, b.value);

            // B -> B
            fsm.Transition(b, 20);

            Assert.AreEqual(b, fsm.currentState);
            Assert.AreEqual(20, b.value);
        }

        // ステートマシンの入れ子
        [Test]
        public void Test_SubStateMachine()
        {
            int order = 0;

            StateMachine fsm  = new StateMachine();
            M            sub1 = new M();
            M            sub2 = new M();

            B sub1b = new B();
            B sub2b = new B();

            fsm.RegisterState(sub1);
            fsm.RegisterState(sub2);

            sub1.RegisterState(sub1b);
            sub2.RegisterState(sub2b);

            Assert.AreEqual(fsm.entryState, fsm.currentState);

            // Entry -> sub1
            fsm.Transition(sub1);

            Assert.AreEqual(sub1, fsm.currentState);
            Assert.AreEqual(sub1.entryState, (fsm.currentState as StateMachine).currentState);

            sub1.Transition(sub1b);

            Assert.AreEqual(sub1, fsm.currentState);
            Assert.AreEqual(sub1b, (fsm.currentState as StateMachine).currentState);
        }

        class A : State
        {
            public Action         InitializeParamsFunc;
            public Action<object> OnEnterFunc;
            public Action         OnExitFunc;

            public override void InitializeParams()
            {
                InitializeParamsFunc?.Invoke();
            }

            protected override void OnEnterInner(object input)
            {
                OnEnterFunc?.Invoke(input);
            }

            protected override void OnExitInner()
            {
                OnExitFunc?.Invoke();
            }
        }

        class B : State
        {
            public int value;

            public Action         InitializeParamsFunc;
            public Action<object> OnEnterFunc;
            public Action         OnExitFunc;

            public override void InitializeParams()
            {
                value = 100;
                InitializeParamsFunc?.Invoke();
            }

            protected override void OnEnterInner(object input)
            {
                if (input != null)
                {
                    this.value = (int) input;
                }

                OnEnterFunc?.Invoke(input);
            }

            protected override void OnExitInner()
            {
                OnExitFunc?.Invoke();
            }
        }

        class M : StateMachine
        {
            public int a;
        }
    }
}