using System;
using UnityEditor;
using UnityEngine;

namespace KoheiUtils
{
    public class CreateAssetsJob
    {
        public ConvertSetting settings;
        public string settingPath => settings.GetDirectoryPath();

        public CreateAssetsJob(ConvertSetting settings)
        {
            this.settings    = settings;
        }

        public void Execute()
        {
            GlobalCCSettings gSettings = CCLogic.GetGlobalSettings();

            try
            {
                CsvConvert.CreateAssets(settings, gSettings);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }
    }
}