namespace KoheiUtils
{
    using UnityEngine;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif
    public class LocalizationManager : SingletonMonoBehaviour<LocalizationManager>
    {
        [SerializeField] LocalizationTable[] tables;

        public bool setupOnAwake = true;
        public bool setDefaultLanguageOnAwake;

#if ODIN_INSPECTOR
        [ShowIf("setDefaultLanguageOnAwake")]
        [ReadOnly]
#endif
        public string defaultLanguage;

        [SerializeField] private LocalizationLookUpTable _lookUpTable;

        public LocalizationLookUpTable lookUpTable => _lookUpTable;

        public string usingLangCode => lookUpTable.usingLangCode;

        private new void Awake()
        {
            base.Awake();

            if (setupOnAwake)
            {
                Setup();
            }
            
            if (setDefaultLanguageOnAwake)
            {
                _lookUpTable.SetLanguage(defaultLanguage);
            }
        }

        public void Setup()
        {
            _lookUpTable = new LocalizationLookUpTable();

            for (int i = 0; i < tables.Length; i++)
            {
                _lookUpTable.AddLocalizationTable(tables[i]);
            }
        }

        public void AddTable(LocalizationTable table)
        {
            _lookUpTable.AddLocalizationTable(table);
        }

        /// <summary>
        /// 例) ja, en, zh-cn(簡体字), zh-tw(繁体字), ko など
        /// </summary>
        public void SetLanguage(string languageCode)
        {
            _lookUpTable.SetLanguage(languageCode);
        }

        public bool ContainsKey(string key)
        {
            return _lookUpTable.ContainsKey(key);
        }

        public string Get(string key)
        {
            return _lookUpTable.Get(key);
        }

        /// <summary>
        /// key が存在しないときは, defaultVal を返す.
        /// </summary>
        public string Get(string key, string defaultVal)
        {
            return _lookUpTable.Get(key, defaultVal);
        }

        public string Format(string key, params object[] args)
        {
            return _lookUpTable.Format(key, args);
        }

        public bool TryGetValue(string key, out string value)
        {
            return _lookUpTable.TryGetValue(key, out value);
        }

        public Sprite GetSprite(string key)
        {
            return _lookUpTable.GetSprite(key);
        }

        public Sprite GetSprite(string key, Sprite defaultSprite)
        {
            return _lookUpTable.GetSprite(key, defaultSprite);
        }

#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameObject/KoheiUtils/LocalizationManager", false, priority = 30)]
        public static void Create(UnityEditor.MenuCommand menuCommand)
        {
            GameObject go = new GameObject("LocalizationManager");
            go.AddComponent<LocalizationManager>();
            
            // Ensure it gets reparented if this was a context click (otherwise does nothing)
            UnityEditor.GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
            // Create a custom game object
            // Register the creation in the undo system
            UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
            
            UnityEditor.Selection.activeObject = go;
        }
#endif
    }
}