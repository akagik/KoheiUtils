using System.Collections.Generic;

namespace KoheiUtils
{
    public class FlipAnimator : FlipAnimatorBase
    {
        protected Dictionary<int, FlipAnimInfo> key2index = new Dictionary<int, FlipAnimInfo>();

        public override FlipAnimInfo GetFlipAnimInfo(int key)
        {
            return key2index.Get(key);
        }
        
        public override bool HasAnimation(int index)
        {
            return key2index.ContainsKey(index);
        }
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods", false)]
        [Sirenix.OdinInspector.Button]
#endif
        public void AddNewAnimation(int newKey, FlipAnimInfo info)
        {
            key2index[newKey] = info;
        }

#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods", false)]
        [Sirenix.OdinInspector.Button]
#endif
        public void RemoveAnimation(int newKey)
        {
            key2index.Remove(newKey);
        }
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.FoldoutGroup("Methods", false)]
        [Sirenix.OdinInspector.Button]
#endif
        public void Clear()
        {
            key2index.Clear();
        }

    }
}