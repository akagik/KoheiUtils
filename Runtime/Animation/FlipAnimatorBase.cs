using UnityEngine;
using System;
using System.Collections.Generic;

namespace KoheiUtils
{
    [RequireComponent(typeof(FlipAnimation))]
    public class FlipAnimatorBase : MonoBehaviour
    {
        [SerializeField] bool playOnStart = true;

        public Action<string> onEventTriggered
        {
            get => animation.onEventTriggered;
            set => animation.onEventTriggered = value;
        }
        
        /// <summary>
        /// アニメーションが完了したときに呼び出される.
        /// </summary>
        private Action onComplete;
        
        /// <summary>
        /// アニメーションが終了したときに呼び出される.
        /// 途中で中断しても呼び出される.
        /// </summary>
        private Action onEnd;

        // -1 のときは、単発アニメーションを再生しても、デフォルトループに戻らない.
        private int       defaultLoopAnimationIndex = 0;
        private List<int> animationStacks           = new List<int>();

        public  int  currentAnimIndex { get; private set; } = -1;
        private bool loop;

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods", false)]
        [Sirenix.OdinInspector.Button]
#endif
        public void SetDefaultLoopAnimation(int animationIndex)
        {
            this.defaultLoopAnimationIndex = animationIndex;
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods")]
        [Sirenix.OdinInspector.Button]
#endif
        public void PlayDefault(Action onComplete = null, Action onEnd = null)
        {
            if (defaultLoopAnimationIndex != -1)
            {
                PlayLoop(defaultLoopAnimationIndex, onComplete, onEnd);
            }
        }

        // ---------------------------------
        // Virtual methods
        // ---------------------------------
        public virtual FlipAnimInfo GetFlipAnimInfo(int key)
        {
            return null;
        }
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods")]
        [Sirenix.OdinInspector.Button]
#endif
        public void PlayLoop(int animationIndex, Action onComplete = null, Action onEnd = null)
        {
            var info = GetFlipAnimInfo(animationIndex);

            if (!ReferenceEquals(info, null))
            {
                this.onComplete = onComplete;
                this.onEnd?.Invoke();
                this.onEnd = onEnd;
                
                animation.Set(info);
                animation.SetLoopCount(-1);
                animation.Play();

                currentAnimIndex = animationIndex;
                loop             = true;
            }
            else
            {
                Debug.LogWarning("指定のアニメーション index は存在しない: " + animationIndex);
            }
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods")]
        [Sirenix.OdinInspector.Button]
#endif
        public void Play(int animationIndex, Action onComplete = null, Action onEnd = null)
        {
            var info = GetFlipAnimInfo(animationIndex);

            if (!ReferenceEquals(info, null))
            {
                this.onComplete = onComplete;
                this.onEnd?.Invoke();
                this.onEnd = onEnd;
                
                animation.Set(info);
                animation.SetLoopCount(1);
                animation.Play();

                currentAnimIndex = animationIndex;
                loop             = false;
            }
            else
            {
                Debug.LogWarning("指定のアニメーション index は存在しない: " + animationIndex);
            }
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods")]
        [Sirenix.OdinInspector.Button]
#endif
        /// <summary>
        /// 現在再生中のアニメーションの次に再生するアニメーションを予約する.
        /// </summary>
        public void AddAnimation(int newIndex)
        {
            animationStacks.Add(newIndex);

            // 現在ループアニメーション再生中の場合は即座にチェックする.
            if (loop)
            {
                CheckStacks();
            }
        }

        private bool CheckStacks()
        {
            if (animationStacks.Count > 0)
            {
                var next = animationStacks[0];
                Play(next);
                animationStacks.RemoveAt(0);
                return true;
            }

            return false;
        }

        private void Awake()
        {
            animation.autoUpdate  = false;
            animation.playOnStart = false;
        }

        private void Start()
        {
            if (playOnStart)
            {
                PlayDefault();
            }
        }

        void Update()
        {
            if (animation.OnUpdate() && !loop)
            {
                onComplete?.Invoke();
                onComplete = null;
                
                onEnd?.Invoke();
                onEnd = null;

                if (!CheckStacks())
                {
                    PlayDefault();
                }
            }
        }

        // cached components
        private FlipAnimation _animation;

        public FlipAnimation animation
        {
            get
            {
                if (_animation == null)
                {
                    _animation = GetComponent<FlipAnimation>();
                }

                return _animation;
            }
        }
    }
}