namespace KoheiUtils
{
    using System.Collections.Generic;
    using UnityEngine;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    public class LocalizationManager : SingletonMonoBehaviour<LocalizationManager>
    {
        [SerializeField] LocalizationTable[] tables;

        public bool setDefaultLanguageOnAwake;

#if ODIN_INSPECTOR
    [ShowIf("setDefaultLanguageOnAwake")]
#endif
        public string defaultLanguage;

#if ODIN_INSPECTOR
        [ReadOnly]
#endif
        [SerializeField]
        private string _usingLanguage = "ja";

        public string usingLangCode => _usingLanguage;

        private Dictionary<string, string> cachedData;

        private new void Awake()
        {
            base.Awake();

            if (setDefaultLanguageOnAwake)
            {
                SetLanguage(defaultLanguage);
            }
        }

        /// <summary>
        /// 例) ja, en, zh-cn(簡体字), zh-tw(繁体字), ko など
        /// </summary>
        public void SetLanguage(string languageCode)
        {
            _usingLanguage = languageCode;
            MakeCache(languageCode);
        }

        public bool ContainsKey(string key)
        {
            Check();
            return cachedData.ContainsKey(key);
        }

        public string Get(string key)
        {
            Check();

#if UNITY_EDITOR
            if (!cachedData.ContainsKey(key))
            {
                Debug.LogError("No key found: " + key);
                return "no text";
            }
#endif
            return cachedData[key];
        }

        /// <summary>
        /// key が存在しないときは, defaultVal を返す.
        /// </summary>
        public string Get(string key, string defaultVal)
        {
            Check();

            if (TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultVal;
        }

        public string Format(string key, params object[] args)
        {
            Check();
            return string.Format(cachedData[key], args);
        }

        public bool TryGetValue(string key, out string value)
        {
            Check();
            return cachedData.TryGetValue(key, out value);
        }

        public Sprite GetSprite(string key)
        {
            Check();

            Sprite sprite = null;

            if (cachedData.TryGetValue(key, out string value))
            {
                sprite = LoadSprite(value);
            }

            return sprite;
        }

        public Sprite GetSprite(string key, Sprite defaultSprite)
        {
            Sprite sprite = GetSprite(key);

            if (sprite == null)
            {
                sprite = defaultSprite;
            }

            return sprite;
        }

        private void Check()
        {
            if (cachedData == null)
            {
                MakeCache(_usingLanguage);
            }
        }

        private void MakeCache(string languageCode)
        {
            cachedData = new Dictionary<string, string>();

            foreach (LocalizationTable table in tables)
            {
                foreach (LocalizationData data in table.rows)
                {
                    if (cachedData.ContainsKey(data.key))
                    {
                        cachedData[data.key] = GetText(data, languageCode);
                    }
                    else
                    {
                        cachedData.Add(data.key, GetText(data, languageCode));
                    }
                }
            }
        }

        private string GetText(LocalizationData data, string languageCode)
        {
            switch (languageCode)
            {
                case "ja":
                    return data.ja;
                case "en":
                    return data.en;
                case "zh-cn":
                    return data.zh_cn;
                default:
                    break;
            }

            Debug.LogErrorFormat("指定の言語コードは存在しません: {0}", languageCode);
            return "";
        }

        public static Sprite LoadSprite(string path)
        {
            if (path == "")
            {
                return null;
            }

            var asset = Resources.Load<Sprite>(path);

            return asset;
        }
    }
}