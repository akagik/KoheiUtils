namespace KoheiUtils
{
    using System;
    using UnityEngine;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// 連番アニメーションを再生する.
    /// Image, SpriteRenderer の両方に対応できるように抽象クラスとして定義.
    /// </summary>
    public abstract class FlipAnimation : MonoBehaviour
    {
        // 初期化・更新設定
        [Header("Initialize / Update Config")]
        public bool playOnStart;

        public bool autoUpdate;

        // 事前に設定されるべき情報.
        [Header("Animation")]
        [SerializeField] float secPerSpr;

        [SerializeField] int      loopCount = -1;
        [SerializeField] Sprite[] sprites;
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.PropertySpace(0, 10f)]
#endif
        [SerializeField] FlipAnimationEventTrigger[] triggers;

        // アニメーション中に利用される変数
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Read only variables", false)]
        [Sirenix.OdinInspector.ReadOnly]
#endif
        [SerializeField] private int leftLoopCount = -1;

        float elappsedSeconds;
        float nextUpdateSeconds;
        int   _currentIndex;
        bool  _isPaused = true;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Read only variables", false)]
        [Sirenix.OdinInspector.ReadOnly]
#endif
        [SerializeField] private int nextEventTriggerIndex = -1;

        public Action         onComplete;
        public Action<string> onEventTriggered;

        public int  currentIndex => _currentIndex;
        public bool isPaused     => _isPaused;

        void Start()
        {
            if (playOnStart)
            {
                Play();
            }
        }

        public virtual void Setup()
        {
            // do nothing
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods", false)]
        [Sirenix.OdinInspector.Button]
#endif
        public void Set(FlipAnimInfo info)
        {
            SetSprites(info.sprites);
            SetTriggers(info.triggers);
            this.secPerSpr = info.secPerFrame;
        }

        public virtual void SetSprites(Sprite[] sprites)
        {
            this.sprites = sprites;
            Rewind();
        }

        public virtual void SetSprites(Sprite[] sprites, float secPerSpr)
        {
            this.sprites   = sprites;
            this.secPerSpr = secPerSpr;
            Rewind();
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods")]
        [Sirenix.OdinInspector.Button]
#endif
        /// <summary>
        /// イベントトリガーをセットする.
        /// noSort が false のときはトリガーをソートしない. ソートしない場合は必ず、トリガーの配列が index 順になっているように
        /// しなければいけない.
        /// </summary>
        public void SetTriggers(FlipAnimationEventTrigger[] triggers, bool noSort = true)
        {
            this.triggers = triggers;

            if (!noSort)
            {
                GenericUtils.InsertSort(triggers);
            }

            CheckNextEventTrigger();
        }

        private void CheckNextEventTrigger(int startEventTriggerIndex = 0)
        {
            if (triggers.Length == 0)
            {
                nextEventTriggerIndex = -1;
                return;
            }

            for (int i = startEventTriggerIndex; i < triggers.Length; i++)
            {
                if (_currentIndex <= triggers[i].index)
                {
                    nextEventTriggerIndex = i;
                    return;
                }
            }

            nextEventTriggerIndex = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckTriggerFired()
        {
            if (nextEventTriggerIndex == -1)
            {
                return;
            }

            if (_currentIndex == triggers[nextEventTriggerIndex].index)
            {
//                Debug.Log("Fire [" + triggers[nextEventTriggerIndex].name + "]");
                onEventTriggered?.Invoke(triggers[nextEventTriggerIndex].name);
                CheckNextEventTrigger(nextEventTriggerIndex + 1);
            }
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods")]
        [Sirenix.OdinInspector.Button]
#endif
        public void SetSecPerSpr(float newSecPerSpr)
        {
            this.secPerSpr = newSecPerSpr;
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods")]
        [Sirenix.OdinInspector.Button]
#endif
        public void SetLoopCount(int newLoopCount = -1)
        {
            this.loopCount     = newLoopCount;
            this.leftLoopCount = loopCount;
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods")]
        [Sirenix.OdinInspector.Button]
#endif
        public void Pause()
        {
            _isPaused = true;
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods")]
        [Sirenix.OdinInspector.Button]
#endif
        public void Kill(bool complete = true)
        {
            Pause();

            if (complete)
            {
                leftLoopCount = loopCount;
                var _onComplete = onComplete;
                onComplete = null;
                _onComplete?.Invoke();
            }
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods")]
        [Sirenix.OdinInspector.Button]
#endif
        public void Play()
        {
            _isPaused          = false;
            this.leftLoopCount = loopCount;

            SetSprite(sprites[_currentIndex]);
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods")]
        [Sirenix.OdinInspector.Button]
#endif
        public void Toggle()
        {
            if (isPaused)
            {
                Play();
            }
            else
            {
                Pause();
            }
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods")]
        [Sirenix.OdinInspector.Button]
#endif
        public void Restart()
        {
            elappsedSeconds    = 0;
            _currentIndex      = 0;
            _isPaused          = false;
            this.leftLoopCount = loopCount;

            SetSprite(sprites[_currentIndex]);
            CheckNextEventTrigger();
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods")]
        [Sirenix.OdinInspector.Button]
#endif
        public void PlayOnce()
        {
            Restart();
            this.loopCount     = 1;
            this.leftLoopCount = 1;
        }

        public void Rewind()
        {
            elappsedSeconds    = 0;
            _currentIndex      = 0;
            _isPaused          = true;
            this.leftLoopCount = loopCount;

            SetSprite(sprites[_currentIndex]);
            CheckNextEventTrigger();
        }

        void Update()
        {
            if (autoUpdate)
            {
                OnUpdate();
            }
        }

        // アニメーションが1周したときは True を返す.
        public bool OnUpdate()
        {
            if (_isPaused)
            {
                return false;
            }

            elappsedSeconds += Time.deltaTime;
            if (elappsedSeconds >= secPerSpr)
            {
                elappsedSeconds = 0f;
                _currentIndex   = (_currentIndex + 1) % sprites.Length;

                CheckTriggerFired();
                bool isEnd = _currentIndex == 0;

                if (isEnd)
                {
                    leftLoopCount--;

                    if (leftLoopCount == 0)
                    {
                        Kill(true);
                        return true;
                    }
                }

                SetSprite(sprites[_currentIndex]);

                return isEnd;
            }

            return false;
        }

        /// <summary>
        /// Sprite をセットする.
        /// SpriteRenderer、Image、Material などに対応できるように
        /// メソッドを abstract にしている.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public abstract void SetSprite(Sprite sprite);
    }
}