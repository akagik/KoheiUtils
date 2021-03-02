namespace KoheiUtils
{
    using System;

    [System.Serializable]
    public class FlipAnimationEventTrigger : IComparable<FlipAnimationEventTrigger>
    {
        public int    index;
        public string name;

        public int CompareTo(FlipAnimationEventTrigger other)
        {
            if (this.index < other.index)
            {
                return 1;
            }
            else if (this.index > other.index)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }
    }
}