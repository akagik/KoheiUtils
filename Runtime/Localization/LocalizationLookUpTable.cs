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
        // key -> (ローカライズ文字列, SmartFormat を使う場合は True)
        private Dictionary<string, (string, bool)> cachedData;
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
            cachedData = new Dictionary<string, (string, bool)>();
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
            }
#endif
            if (cachedData.TryGetValue(key, out var val))
            {
                return val.Item1;
            }

            return string.Empty;
        }

        /// <summary>
        /// key が存在しないときは, defaultVal を返す.
        /// </summary>
        public string Get(string key, string defaultVal)
        {
            if (cachedData.TryGetValue(key, out var val))
            {
                return val.Item1;
            }

            return defaultVal;
        }

        public string Format(string key, params object[] args)
        {
#if UNITY_EDITOR
            if (!cachedData.ContainsKey(key))
            {
                Debug.LogError("No key found: " + key);
            }
#endif
            
            if (cachedData.TryGetValue(key, out var valAndSmart))
            {
                // Smart の場合.
                if (valAndSmart.Item2)
                {
                    return SmartFormat.Smart.Format(valAndSmart.Item1, args);
                }
                return string.Format(valAndSmart.Item1, args);
            }

            return string.Empty;
        }

        public bool TryGetValue(string key, out string strValue)
        {
            if (cachedData.TryGetValue(key, out var val))
            {
                strValue = val.Item1;
                return true;
            }

            strValue = string.Empty;
            return false;
        }

        public Sprite GetSprite(string key)
        {
            Sprite sprite = null;

            if (cachedData.TryGetValue(key, out var val))
            {
                sprite = LoadSprite(val.Item1);
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
                var val = (GetText(data, languageCode), data.smart);
                if (cachedData.ContainsKey(data.key))
                {
                    cachedData[data.key] = val;
                }
                else
                {
                    cachedData.Add(data.key, val);
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
                case "zh-tw":
                    return data.zh_tw;
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