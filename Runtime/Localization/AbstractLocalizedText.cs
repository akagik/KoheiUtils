namespace KoheiUtils
{
    using UnityEngine;
    using UnityEngine.UI;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    public abstract class AbstractLocalizedText : MonoBehaviour
    {
        public string key;
        public bool   setOnStart = true;

        public abstract string text { get; }

        public void Start()
        {
            if (setOnStart)
            {
                if (!LocalizationManager.Instance.ContainsKey(key))
                {
                    Debug.LogError("指定のキーは存在しない: " + key + ", GameObject.name: " + gameObject.name);
                    return;
                }

                UpdateText();
            }
        }

        public abstract void UpdateText();

#if UNITY_EDITOR && ODIN_INSPECTOR
    [Button]
    public void UpdateTextButton()
    {
        var manager = FindObjectOfType<LocalizationManager>();

        if(manager == null)
        {
            Debug.LogError("LocalizationManager がシーン内に存在しません");
            return;
        }

        if(manager.setDefaultLanguageOnAwake)
        {
            manager.SetLanguage(manager.defaultLanguage);
        }
        UpdateText();
    }
#endif

    }
}
