namespace KoheiUtils
{
    using UnityEngine;

#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
#endif

    public abstract class AbstractLocalizedSprite : MonoBehaviour
    {
        public string key;
        public bool   setOnStart = true;

        public abstract Sprite sprite { get; }

        public void Start()
        {
            if (setOnStart)
            {
                if (!LocalizationManager.Instance.ContainsKey(key))
                {
                    Debug.LogError("指定のキーは存在しない: " + key + ", GameObject.name: " + gameObject.name);
                    return;
                }

                UpdateSprite();
            }
        }

        public abstract void UpdateSprite();

#if UNITY_EDITOR && ODIN_INSPECTOR
    [Button]
    public void UpdateSpriteButton()
    {
        var manager = FindObjectOfType<LocalizationManager>();

        if (manager == null)
        {
            Debug.LogError("LocalizationManager がシーン内に存在しません");
            return;
        }

        if (manager.setDefaultLanguageOnAwake)
        {
            manager.SetLanguage(manager.defaultLanguage);
        }
        UpdateSprite();
    }
#endif
    }
}
