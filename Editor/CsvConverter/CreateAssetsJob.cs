using System;
using UnityEditor;
using UnityEngine;

namespace KoheiUtils
{
    public class CreateAssetsJob
    {
        public CsvConverterSettings.Setting settings;
        public string                       settingPath;

        public CreateAssetsJob(CsvConverterSettings.Setting settings, string settingPath)
        {
            this.settings    = settings;
            this.settingPath = settingPath;
        }

        public void Execute()
        {
            GlobalCCSettings gSettings = CCLogic.GetGlobalSettings();

            try
            {
                CsvConvert.CreateAssets(settings, gSettings, settingPath);
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