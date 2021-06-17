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

        // -1 のときは、単発アニメーションを再生しても、デフォルトループに戻らない.
        private int              defaultLoopAnimationIndex = 0;
        private List<TrackEntry> animationStacks           = new List<TrackEntry>();

        TrackEntry        _currentTrackEntry = new TrackEntry {animationIndex = -1};
        public TrackEntry currentTrackEntry => _currentTrackEntry;

        public int currentAnimIndex => _currentTrackEntry.animationIndex;
        bool       loop             => _currentTrackEntry.loop;

        // ---------------------------------
        // Virtual methods
        // ---------------------------------
        public virtual FlipAnimInfo GetFlipAnimInfo(int key)
        {
            return null;
        }

        public virtual bool HasAnimation(int index)
        {
            return false;
        }

        // ---------------------------------
        // Public methods
        // ---------------------------------
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

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods")]
        [Sirenix.OdinInspector.Button]
#endif
        public void Play(TrackEntry entry)
        {
            var info = GetFlipAnimInfo(entry.animationIndex);

            if (!ReferenceEquals(info, null))
            {
                // onEnd で Play は挟まる可能性があるので、すべての onEnd が消化されるまで繰り返す.
                // 最終的には onEnd での Play は 最初の Play で上書きされる.
                while (_currentTrackEntry.onEnd != null)
                {
                    Action _onEnd = _currentTrackEntry.onEnd;
                    _currentTrackEntry.onEnd = null;
                    _onEnd?.Invoke();
                }

                _currentTrackEntry = entry;

                animation.Set(info);
                animation.SetLoopCount(entry.loop ? -1 : 1);
                animation.Play();
            }
            else
            {
                Debug.LogWarning("指定のアニメーション index は存在しない: " + entry.animationIndex);
            }
        }
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods")]
        [Sirenix.OdinInspector.Button]
#endif
        public void Play(int animationIndex, bool loop = false, Action onComplete = null, Action onEnd = null)
        {
            var info = GetFlipAnimInfo(animationIndex);

            if (!ReferenceEquals(info, null))
            {
                // onEnd で Play は挟まる可能性があるので、すべての onEnd が消化されるまで繰り返す.
                // 最終的には onEnd での Play は 最初の Play で上書きされる.
                while (_currentTrackEntry.onEnd != null)
                {
                    Action _onEnd = _currentTrackEntry.onEnd;
                    _currentTrackEntry.onEnd = null;
                    _onEnd?.Invoke();
                }

                _currentTrackEntry                = new TrackEntry();
                _currentTrackEntry.animationIndex = animationIndex;
                _currentTrackEntry.loop           = loop;
                _currentTrackEntry.onEnd          = onEnd;
                _currentTrackEntry.onCompleted    = onComplete;
                
                animation.Set(info);
                animation.SetLoopCount(loop ? -1 : 1);
                animation.Play();
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
        public void PlayLoop(int animationIndex, Action onComplete = null, Action onEnd = null)
        {
            Play(animationIndex, true, onComplete, onEnd);
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods")]
        [Sirenix.OdinInspector.Button]
#endif
        /// <summary>
        /// 現在再生中のアニメーションの次に再生するアニメーションを予約する.
        /// </summary>
        public void AddAnimation(int newIndex, bool loop = false, Action onComplete = null, Action onEnd = null)
        {
            var addedEntry = new TrackEntry();
            addedEntry.animationIndex = newIndex;
            addedEntry.loop           = false;
            addedEntry.onEnd          = onEnd;
            addedEntry.onCompleted    = onComplete;

            animationStacks.Add(addedEntry);

            // 現在ループアニメーション再生中の場合は即座にチェックする.
            if (loop)
            {
                CheckStacks();
            }
        }

        public void ClearStacks()
        {
            animationStacks.Clear();
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
            if (animation.OnUpdate())
            {
                _currentTrackEntry.onCompleted?.Invoke();
                
                if (!loop)
                {
                    // NOTE: OnEnd について1回コールされると、2度コールされないようにする.
                    // NOTE: コールする前に null を代入しないと、 onEnd コール中に遷移が発生し、StackOverFlow が起こる可能性がある.
                    if (_currentTrackEntry.onEnd != null)
                    {
                        Action onEnd = _currentTrackEntry.onEnd;
                        _currentTrackEntry.onEnd = null;
                        onEnd.Invoke();
                    }
                    
                    if (!CheckStacks())
                    {
                        PlayDefault();
                    }
                }
            }
        }

        // cached components
        private FlipAnimation _animation;

        public FlipAnimation animation
        {
            get
            {
                if (ReferenceEquals(_animation, null))
                {
                    _animation = GetComponent<FlipAnimation>();
                }

                return _animation;
            }
        }
    }
}