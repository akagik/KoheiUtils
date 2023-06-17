using System;

namespace KoheiUtils
{
    using UnityEngine;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;

#endif

    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] PoolElement template;
        [SerializeField] bool        setupOnAwake = true;
        [SerializeField] bool        callUpdate = false;
        [SerializeField] int         poolSize     = 10;

        [SerializeField] bool inactiveOnReturn = true; // 返却時は gameObject を非アクティブ化するか？
        [SerializeField] bool forceReturn;             // 使用中のエレメントを強制リターンするか？

        [Tooltip("Scale up this pool array if all elements are rent.")] [SerializeField]
        bool dynamicScale; // 要素が足りないときにプールを拡張するか？

        [SerializeField] int dynamicScaleSize = 5; // 要素が足りないときにどれだけ足すか？

        [Header("Read only variables")]
#if ODIN_INSPECTOR
        [ReadOnly]
#endif
        [SerializeField]
        private int currentIndex;

#if ODIN_INSPECTOR
        [ReadOnly]
#endif
        [SerializeField]
        int _currentRentSize;

        public int currentRentSize => _currentRentSize;

#if ODIN_INSPECTOR
        [ReadOnly]
#endif
        [SerializeField] PoolElement[] elements;

        void Awake()
        {
            if (setupOnAwake)
            {
                Setup();
            }
        }

        protected virtual PoolElement Instantiate(int i)
        {
            var element = Instantiate(template, transform);
            element.Return();
            element.gameObject.name = template.gameObject.name + " " + i;
            element.Setup(OnElementReturn);
            
            if (inactiveOnReturn)
            {
                element.gameObject.SetActive(false);
            }
            return element;
        }

        public void Setup()
        {
            _currentRentSize = 0;
            currentIndex     = 0;
            elements         = new PoolElement[poolSize];

            for (int i = 0; i < elements.Length; i++)
            {
                elements[i] = Instantiate(i);
            }

            if (inactiveOnReturn && template.gameObject.scene.IsValid())
            {
                template.gameObject.SetActive(false);
            }

            currentIndex = elements.Length - 1;
        }

        public T Rent<T>() where T : PoolElement
        {
            return Rent().GetComponent<T>();
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public PoolElement Rent()
        {
            currentIndex = (++currentIndex) % elements.Length;
            int startIndex = currentIndex;

            do
            {
                var p = elements[currentIndex];

                if (p.isRent)
                {
                    if (forceReturn)
                    {
                        p.Return();
                        p.Rent();
                        if (inactiveOnReturn) p.gameObject.SetActive(true);
                        _currentRentSize++;
                        return p;
                    }
                }
                else
                {
                    p.Rent();
                    if (inactiveOnReturn) p.gameObject.SetActive(true);
                    _currentRentSize++;
                    return p;
                }

                currentIndex = (++currentIndex) % elements.Length;
            } while (currentIndex != startIndex);

            // 空いている要素がなかった場合.
            if (dynamicScale && dynamicScaleSize > 0)
            {
                int oldElementsSize = elements.Length;

                ScaleUp(dynamicScaleSize);

                var p = elements[oldElementsSize];
                p.Rent();
                if (inactiveOnReturn) p.gameObject.SetActive(true);
                _currentRentSize++;
                currentIndex = oldElementsSize;

                return p;
            }

            return null;
        }

#if ODIN_INSPECTOR
        [Button]
#endif
        public void ScaleUp(int size)
        {
            if (size <= 0)
            {
                Debug.LogError("Invalid size: " + size + ". size must be greater than 0.");
                return;
            }

            var elementsTemp = elements;
            int oldSize      = elementsTemp.Length;

            poolSize = elements.Length + size;
            elements = new PoolElement[poolSize];

            Array.Copy(elementsTemp, elements, oldSize);

            for (int i = oldSize; i < elements.Length; i++)
            {
                elements[i] = Instantiate(i);
            }
        }

        private void OnElementReturn(PoolElement returnedElement)
        {
            _currentRentSize--;
            if (inactiveOnReturn)
            {
                returnedElement.gameObject.SetActive(false);
            }
        }

        public void ReturnAll()
        {
            _currentRentSize = 0;

            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i].isRent)
                {
                    elements[i].Return();

                    if (inactiveOnReturn)
                    {
                        elements[i].gameObject.SetActive(false);
                    }
                }
            }

            currentIndex = elements.Length - 1;
        }
        
        public T First<T>(Func<T, bool> cond) where T :PoolElement
        {
            for (int i = 0; i < elements.Length; i++)
            {
                var elem = elements[i];
                if (elem.isRent)
                {
                    var elemT = elem.GetComponent<T>();
                    if (cond(elemT))
                    {
                        return elemT;
                    }
                }
            }

            return null;
        }
        
        public void ForEach<T>(Action<T> action) where T : PoolElement
        {
            for (int i = 0; i < elements.Length; i++)
            {
                var elem = elements[i];
                if (elem.isRent)
                {
                    var elemT = elem.GetComponent<T>();
                    action(elemT);
                }
            }
        }

        void Update()
        {
            if (!callUpdate)
            {
                return;
            }
            
            for (int i = 0; i < elements.Length; i++)
            {
                var elem = elements[i];
                if (elem.isRent)
                {
                    elem.OnUpdate();
                }
            }
        }
    }
}