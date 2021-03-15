using System;
using System.Collections.Generic;

namespace KoheiUtils
{
    using UnityEngine;

    [RequireComponent(typeof(FlipAnimation))]
    public class FlipAnimator : MonoBehaviour
    {
        [Sirenix.OdinInspector.PropertySpace(0, 10f)]
        public FlipAnimationController controller;

        public Action onComplete;

        public Action<string> onEventTriggered
        {
            get => animation.onEventTriggered;
            set => animation.onEventTriggered = value;
        }

        // -1 のときは、単発アニメーションを再生しても、デフォルトループに戻らない.
        private int       defaultLoopAnimationIndex = 0;
        private List<int> animationStacks           = new List<int>();

        public int currentAnimIndex { get; private set; } = -1;
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
        public void PlayDefault()
        {
            if (defaultLoopAnimationIndex != -1)
            {
                PlayLoop(defaultLoopAnimationIndex);
            }
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods")]
        [Sirenix.OdinInspector.Button]
#endif
        public void PlayLoop(int animationIndex)
        {
            animation.Set(controller.animations[animationIndex]);
            animation.SetLoopCount(-1);
            animation.Play();

            currentAnimIndex = animationIndex;
            loop             = true;
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods")]
        [Sirenix.OdinInspector.Button]
#endif
        public void Play(int animationIndex)
        {
            animation.Set(controller.animations[animationIndex]);
            animation.SetLoopCount(1);
            animation.Play();
            
            currentAnimIndex = animationIndex;
            loop             = false;
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
            animation.autoUpdate = false;
            animation.playOnStart = false;
        }
        
        private void Start()
        {
            PlayDefault();
        }

        void Update()
        {
            if (animation.OnUpdate() && !loop)
            {
                onComplete?.Invoke();
                onComplete = null;
                
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