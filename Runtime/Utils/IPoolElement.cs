namespace KoheiUtils
{
    using System;

    public interface IPoolElement
    {
        public bool isRent { get; }
        Action<IPoolElement> onReturn { get; }

        public void Setup(Action<IPoolElement> onReturn);

        public void Rent();
        public void Return();
        public void OnUpdate();
    }
}