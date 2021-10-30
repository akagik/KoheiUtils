using UnityEngine;

namespace KoheiUtils
{
    using UnityEditor;
#if ODIN_INSPECTOR
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;

    public class CCSettingsEditWindow : OdinEditorWindow
    {
        public string className;
        public string sheetId;
        public string gid;
        public string key;
        public bool   isDictionary;

//        string                       settingPath;
        ConvertSetting settings;

        public static CCSettingsEditWindow OpenWindow()
        {
            var window = GetWindow<CCSettingsEditWindow>();
            window.Show();
            return window;
        }

        public void SetSettings(ConvertSetting setting)
        {
//            this.settingPath = settingPath;
            this.settings = setting;

            className    = setting.className;
            sheetId      = setting.sheetID;
            gid          = setting.gid;
            key          = setting.key;
            isDictionary = setting.isDictionary;
        }

        [Button(ButtonSizes.Large), GUIColor(0, 1, 0)]
        public void SaveSettings()
        {
            settings.className    = className;
            settings.sheetID      = sheetId;
            settings.gid          = gid;
            settings.key          = key;
            settings.isDictionary = isDictionary;

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Close();
        }
    }

#else
    public class CCSettingsEditWindow : EditorWindow
    {
    }
#endif
}