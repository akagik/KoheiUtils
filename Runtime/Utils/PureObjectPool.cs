using System;
using System.Collections.Generic;
using ElementClass = KoheiUtils.IPoolElement;

namespace KoheiUtils
{
    using UnityEngine;

    public class PureObjectPool<T> where T : class, ElementClass, new()
    {
        readonly bool forceReturn;
        readonly bool dynamicScale; // 要素が足りないときにプールを拡張するか？
        readonly int dynamicScaleSize = 5; // 要素が足りないときにどれだけ足すか？
        

        private int currentIndex;
        private int _currentRentSize;
        
        private int poolSize;
        private T[] elements;
        private Queue<ElementClass> availableElements;
        
        public int CurrentRentSize => _currentRentSize;

        public PureObjectPool(int poolSize = 10, bool forceReturn = false, bool dynamicScale = false, int dynamicScaleSize = 5)
        {
            this.poolSize = poolSize;
            this.forceReturn = forceReturn;
            this.dynamicScale = dynamicScale;
            this.dynamicScaleSize = dynamicScaleSize;
            this.elements = new T[poolSize];
            this.availableElements = new Queue<ElementClass>();

            Setup();
        }

        protected virtual T Instantiate(int i)
        {
            var element = new T();
            element.Setup(OnElementReturn);
            return element;
        }

        private void Setup()
        {
            _currentRentSize = 0;
            currentIndex = 0;

            for (int i = 0; i < elements.Length; i++)
            {
                elements[i] = Instantiate(i);
            }

            currentIndex = elements.Length - 1;
        }

        public T Rent()
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
                        _currentRentSize++;
                        return p;
                    }
                }
                else
                {
                    p.Rent();
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
                _currentRentSize++;
                currentIndex = oldElementsSize;

                return p;
            }

            return null;
        }

        public void ScaleUp(int size)
        {
            if (size <= 0)
            {
                Debug.LogError("Invalid size: " + size + ". size must be greater than 0.");
                return;
            }

            var elementsTemp = elements;
            int oldSize = elementsTemp.Length;

            poolSize = elements.Length + size;
            elements = new T[poolSize];

            Array.Copy(elementsTemp, elements, oldSize);

            for (int i = oldSize; i < elements.Length; i++)
            {
                elements[i] = Instantiate(i);
            }
        }

        private void OnElementReturn(ElementClass returnedElement)
        {
            _currentRentSize--;
        }

        public void ReturnAll()
        {
            _currentRentSize = 0;

            for (int i = 0; i < elements.Length; i++)
            {
                if (elements[i].isRent)
                {
                    elements[i].Return();
                }
            }

            currentIndex = elements.Length - 1;
        }

        public T First(Func<T, bool> cond)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                var elem = elements[i];
                if (elem.isRent)
                {
                    if (cond(elem))
                    {
                        return elem;
                    }
                }
            }

            return null;
        }

        public void ForEach(Action<T> action)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                var elem = elements[i];
                if (elem.isRent)
                {
                    action(elem);
                }
            }
        }

        public void Update()
        {
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