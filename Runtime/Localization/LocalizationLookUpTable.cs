namespace KoheiUtils
{
    using System.Collections.Generic;
    using UnityEngine;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;

#endif

    [System.Serializable]
    public class LocalizationLookUpTable
    {
        private Dictionary<string, string> cachedData;
        [SerializeField] List<LocalizationTable> tables;

#if ODIN_INSPECTOR
        [ReadOnly]
#endif
        [SerializeField]
        private string _usingLanguage = "";

        public string usingLangCode => _usingLanguage;

        public LocalizationLookUpTable()
        {
            tables = new List<LocalizationTable>();
            cachedData = new Dictionary<string, string>();
        }

        public void AddLocalizationTable(LocalizationTable newTable)
        {
            tables.Add(newTable);
            MakeCache(usingLangCode, newTable);
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
            return cachedData.ContainsKey(key);
        }

        public string Get(string key)
        {
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
            if (TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultVal;
        }

        public string Format(string key, params object[] args)
        {
            return string.Format(cachedData[key], args);
        }

        public bool TryGetValue(string key, out string value)
        {
            return cachedData.TryGetValue(key, out value);
        }

        public Sprite GetSprite(string key)
        {
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

        private void MakeCache(string languageCode)
        {
            foreach (LocalizationTable table in tables)
            {
                MakeCache(languageCode, table);
            }
        }

        private void MakeCache(string languageCode, LocalizationTable table)
        {
            if (string.IsNullOrEmpty(languageCode))
            {
                return;
            }
            
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