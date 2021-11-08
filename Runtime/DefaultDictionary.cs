using System.Collections.Generic;

namespace KoheiUtils
{
    /// <summary>
    /// https://stackoverflow.com/questions/15622622/analogue-of-pythons-defaultdict
    /// </summary>
    public class DefaultDictionary<TKey, TValue> : Dictionary<TKey, TValue> where TValue : new()
    {
        public DefaultDictionary() : base ()
        {
            
        }
        
        public DefaultDictionary(DefaultDictionary<TKey, TValue> copied) : base (copied)
        {
            
        }

        public new TValue this[TKey key]
        {
            get
            {
                if (!TryGetValue(key, out var val))
                {
                    val = new TValue();
                    Add(key, val);
                }

                return val;
            }
            set => base[key] = value;
        }
    }
}