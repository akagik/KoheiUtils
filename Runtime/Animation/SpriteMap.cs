namespace KoheiUtils
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [CreateAssetMenu(menuName = "KoheiUtils/SpriteMap")]
    public class SpriteMap : ScriptableObject
    {
        public Entry[] elements;

        [NonSerialized]
//    Dictionary<string, Sprite> mapper;
        Dictionary<Sprite, Sprite> mapper;

        public Sprite GetSprite(Sprite pre)
        {
//        if (mapper == null)
//        {
//            mapper = new Dictionary<string, Sprite>();
//
//            for (int i = 0; i < elements.Length; i++)
//            {
//                mapper.Add(elements[i].pre.name, elements[i].post);
//            }
//        }

//        Sprite sprite;
//        if (mapper.TryGetValue(pre.name, out sprite))
//        {
//            return sprite;
//        }

            if (mapper == null)
            {
                mapper = new Dictionary<Sprite, Sprite>();

                for (int i = 0; i < elements.Length; i++)
                {
                    mapper.Add(elements[i].pre, elements[i].post);
                }
            }

            if (mapper.TryGetValue(pre, out var sprite))
            {
                return sprite;
            }

            return pre;
        }

        [Serializable]
        public class Entry
        {
            public Sprite pre;
            public Sprite post;
        }
    }
}