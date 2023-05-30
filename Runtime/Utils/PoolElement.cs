namespace KoheiUtils
{
    using System;
    using UnityEngine;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;

#endif

    public class PoolElement : MonoBehaviour
    {
        public bool isRent { get; private set; }

        // Object Pool で使用される.
        private Action<PoolElement> onReturn;

        public void Setup(Action<PoolElement> onReturn)
        {
            this.onReturn = onReturn;
        }

        public void Rent()
        {
            isRent = true;
            RentInner();
        }

        protected virtual void RentInner()
        {
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public void Return()
        {
            onReturn?.Invoke(this);
            isRent = false;
            ReturnInner();
        }

        protected virtual void ReturnInner()
        {
        }
        
        public virtual void OnUpdate()
        {
        }
    }
}