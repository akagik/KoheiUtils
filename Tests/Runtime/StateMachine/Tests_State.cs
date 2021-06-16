using System;

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

            fsm.Transition(stateA);

            stateA.OnExitFunc = () =>
            {
                Assert.AreEqual(0, order);
                order++;
            };

            stateA.InitializeParamsFunc = () =>
            {
                Assert.AreEqual(1, order);
                order++;
            };

            stateB.OnEnterFunc = () =>
            {
                Assert.AreEqual(2, order);
                order++;
            };

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

            stateA.InitializeParamsFunc = () =>
            {
                Assert.AreEqual(1, order);
                order++;
            };

            b1.OnEnterFunc = () =>
            {
                // b1 には移行しない.
                Assert.Fail();
            };

            b2.OnEnterFunc = () =>
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

            // 登録直後は初期状態
            Assert.AreEqual(100, b.a);

            fsm.Transition(a);

            a.OnExitFunc = () =>
            {
                Assert.AreEqual(0, order);
                order++;
            };

            a.InitializeParamsFunc = () =>
            {
                Assert.AreEqual(1, order);
                order++;
            };

            b.OnEnterFunc = () =>
            {
                Assert.AreEqual(3, order);
                Assert.AreEqual(5, b.a);
                order++;
            };

            Assert.AreEqual(a, fsm.currentState);

            if (fsm.BeginTransition())
            {
                Assert.AreEqual(2, order);
                order++;

                // この段階ではまだ遷移は完了しない.
                Assert.AreEqual(a, fsm.currentState);
                Assert.IsTrue(fsm.isStateChanging);

                Assert.AreEqual(100, b.a);
                b.a = 5;
                Assert.AreEqual(5, b.a);

                fsm.EndTransition(b);

                Assert.AreEqual(b, fsm.currentState);
                Assert.AreEqual(a, fsm.previousState);
                Assert.AreEqual(5, b.a);

                Assert.AreEqual(4, order);
                order++;
            }
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

            // 登録直後は初期状態
            Assert.AreEqual(100, b1.a);
            Assert.AreEqual(100, b2.a);

            fsm.Transition(a);

            a.OnExitFunc = () =>
            {
                Assert.AreEqual(0, order);
                order++;

                fsm.Transition(b2);
            };

            a.InitializeParamsFunc = () =>
            {
                Assert.AreEqual(1, order);
                order++;
            };

            b1.OnEnterFunc = () => { Assert.Fail(); };

            b2.OnEnterFunc = () =>
            {
                Assert.AreEqual(2, order);
                order++;
            };

            Assert.AreEqual(a, fsm.currentState);

            if (fsm.BeginTransition())
            {
                Assert.Fail();
            }
            else
            {
                Assert.AreEqual(3, order);
                order++;

                Assert.AreEqual(b2, fsm.currentState);
                Assert.AreEqual(a, fsm.previousState);
            }
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

            // 登録直後は初期状態
            Assert.AreEqual(100, b1.a);
            Assert.AreEqual(100, b2.a);

            fsm.Transition(a);

            a.OnExitFunc = () =>
            {
                Assert.AreEqual(0, order);
                order++;

                Assert.IsTrue(fsm.isStateChanging);

                if (fsm.BeginTransition())
                {
                    Assert.AreEqual(2, order);
                    order++;

                    b2.a = 15;

                    fsm.EndTransition(b2);

                    Assert.AreEqual(4, order);
                    order++;
                }
                else
                {
                    Assert.Fail();
                }
            };

            a.InitializeParamsFunc = () =>
            {
                Assert.AreEqual(1, order);
                order++;
            };

            b1.OnEnterFunc = () => { Assert.Fail(); };

            b2.OnEnterFunc = () =>
            {
                Assert.AreEqual(3, order);
                order++;

                Assert.AreEqual(15, b2.a);
            };

            Assert.AreEqual(a, fsm.currentState);

            if (fsm.BeginTransition())
            {
                Assert.Fail();
            }
            else
            {
                Assert.AreEqual(5, order);
                order++;

                Assert.AreEqual(b2, fsm.currentState);
                Assert.AreEqual(a, fsm.previousState);
            }
        }

        // パラメータ初期化チェック
        [Test]
        public void Test_InitializeParams()
        {
            int          order = 0;
            StateMachine fsm   = new StateMachine();
            A            a     = new A();
            B            b1    = new B();
            B            b2    = new B();

            fsm.RegisterState(a);
            fsm.RegisterState(b1);
            fsm.RegisterState(b2);

            // 登録直後は初期状態
            Assert.AreEqual(100, b1.a);
            Assert.AreEqual(100, b2.a);

            fsm.Transition(a);

            a.OnExitFunc = () =>
            {
                Assert.AreEqual(0, order);
                order++;
            };

            a.InitializeParamsFunc = () =>
            {
                Assert.AreEqual(1, order);
                order++;
            };

            b1.OnEnterFunc = () =>
            {
                Assert.AreEqual(2, order);
                order++;

                Assert.AreEqual(5, b1.a);
            };

            b1.OnExitFunc = () =>
            {
                Assert.AreEqual(3, order);
                order++;

                Assert.AreEqual(5, b1.a);
            };

            b1.InitializeParamsFunc = () =>
            {
                Assert.AreEqual(4, order);
                order++;
            };

            b2.OnEnterFunc = () =>
            {
                Assert.AreEqual(5, order);
                order++;

                Assert.AreEqual(20, b2.a);
            };

            Assert.AreEqual(a, fsm.currentState);

            // A -> B1 (5)
            if (fsm.BeginTransition())
            {
                b1.a = 5;
                fsm.EndTransition(b1);
            }
            else
            {
                Assert.Fail();
            }

            Assert.AreEqual(5, b1.a);

            // B1 -> B2 (20)
            if (fsm.BeginTransition())
            {
                Assert.AreEqual(100, b1.a);

                b2.a = 20;
                fsm.EndTransition(b2);

                Assert.AreEqual(6, order);
                order++;
            }
            else
            {
                Assert.Fail();
            }

            // B2 -> A
            fsm.Transition(a);
            Assert.AreEqual(100, b2.a);
        }

        // 自己遷移
        [Test]
        public void Test_ChangeToSelf()
        {
            int          order = 0;
            StateMachine fsm   = new StateMachine();
            A            a     = new A();

            fsm.RegisterState(a);

            a.OnEnterFunc = () =>
            {
                Assert.AreEqual(0, order);
                order++;
            };

            a.OnExitFunc = () =>
            {
                Assert.AreEqual(1, order);
                order++;
            };

            a.InitializeParamsFunc = () =>
            {
                Assert.AreEqual(2, order);
                order++;
            };

            // Entry -> A
            fsm.Transition(a);
            Assert.AreEqual(a, fsm.currentState);

            a.OnEnterFunc = () =>
            {
                Assert.AreEqual(3, order);
                order++;
            };

            // A -> A
            fsm.Transition(a);
            Assert.AreEqual(a, fsm.currentState);

            Assert.AreEqual(4, order);
            order++;
        }

        // パラメータ付き自己遷移
        [Test]
        public void Test_ChangeToSelfWithParams()
        {
            int          order = 0;
            StateMachine fsm   = new StateMachine();
            B            b     = new B();

            fsm.RegisterState(b);

            b.OnEnterFunc = () =>
            {
                Assert.AreEqual(0, order);
                order++;

                Assert.AreEqual(10, b.a);
            };

            b.OnExitFunc = () =>
            {
                Assert.AreEqual(1, order);
                order++;

                Assert.AreEqual(10, b.a);
            };

            b.InitializeParamsFunc = () =>
            {
                Assert.AreEqual(2, order);
                order++;
            };

            // Entry -> B
            if (fsm.BeginTransition())
            {
                b.a = 10;
                fsm.EndTransition(b);
            }

            Assert.AreEqual(b, fsm.currentState);
            Assert.AreEqual(10, b.a);

            b.OnEnterFunc = () =>
            {
                Assert.AreEqual(3, order);
                order++;

                Assert.AreEqual(20, b.a);
            };

            // B -> B
            if (fsm.BeginTransition())
            {
                b.a = 20;
                fsm.EndTransition(b);
            }

            Assert.AreEqual(b, fsm.currentState);
            Assert.AreEqual(20, b.a);

            Assert.AreEqual(4, order);
            order++;
        }

        // ステートマシンの入れ子
        [Test]
        public void Test_SubStateMachine()
        {
            int order = 0;

            StateMachine fsm  = new StateMachine();
            M            sub1 = new M();
            M            sub2 = new M();
            
            B            sub1b    = new B();
            B            sub2b    = new B();

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
            public Action InitializeParamsFunc;
            public Action OnEnterFunc;
            public Action OnExitFunc;

            public override void InitializeParams()
            {
                InitializeParamsFunc?.Invoke();
            }

            protected override void OnEnterInner()
            {
                OnEnterFunc?.Invoke();
            }

            protected override void OnExitInner()
            {
                OnExitFunc?.Invoke();
            }
        }

        class B : State
        {
            public int a;

            public Action InitializeParamsFunc;
            public Action OnEnterFunc;
            public Action OnExitFunc;

            public override void InitializeParams()
            {
                a = 100;
                InitializeParamsFunc?.Invoke();
            }

            protected override void OnEnterInner()
            {
                OnEnterFunc?.Invoke();
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