namespace CsvConverter
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System.IO;
    using System.Linq;
    using KoheiUtils;
    using Object = UnityEngine.Object;

    public class CsvConverterWindow : EditorWindow
    {
        private bool    isDownloading;
        private Vector2 scrollPosition;

        // 二重に保存しないようにするために導入
        private string savedGUID;

        // 検索ボックス用
        private static GUIStyle toolbarSearchField;
        private static GUIStyle toolbarSearchFieldCancelButton;
        private static GUIStyle toolbarSearchFieldCancelButtonEmpty;
        private        string   searchTxt = "";

        // チェックボックス用
        CsvConverterSettings.Setting[] cachedAllSettings;
        private string[]               cachedAllSettingsPath;
        private CsvConverterSettings[] cachedAllParentSettings;

        [MenuItem("Window/CsvConverter", false, 0)]
        static public void OpenWindow()
        {
            EditorWindow.GetWindow<CsvConverterWindow>(false, "CsvConverter", true).Show();
        }

        void OnFocus()
        {
            // isAll用のデータをキャッシュ
            var allSettingList       = new List<CsvConverterSettings.Setting>();
            var allSettingPathList   = new List<string>();
            var allParentSettingList = new List<CsvConverterSettings>();

            string[] settingGUIDArray = AssetDatabase.FindAssets("t:CsvConverterSettings");
            for (int i = 0; i < settingGUIDArray.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(settingGUIDArray[i]);
                var    settings  = AssetDatabase.LoadAssetAtPath<CsvConverterSettings>(assetPath);
                allSettingList.AddRange(settings.list);

                string settingPath = Path.GetDirectoryName(assetPath);
                allSettingPathList.AddRange(Enumerable.Repeat(settingPath, settings.list.Length));
                allParentSettingList.AddRange(Enumerable.Repeat(settings, settings.list.Length));
            }

            cachedAllSettings       = allSettingList.ToArray();
            cachedAllSettingsPath   = allSettingPathList.ToArray();
            cachedAllParentSettings = allParentSettingList.ToArray();
        }

        private void OnGUI()
        {
            GUILayout.Space(6f);
            CsvConverterSettings.Setting[] setting        = null;
            CsvConverterSettings[]         parentSettings = null;
            string[]                       settingPaths   = null;

            if (cachedAllSettings != null)
            {
                setting        = cachedAllSettings;
                settingPaths   = cachedAllSettingsPath;
                parentSettings = cachedAllParentSettings;
            }

            // 検索ボックスを表示
            GUILayout.BeginHorizontal();
            searchTxt = SearchField(searchTxt);
            searchTxt = searchTxt.ToLower();
            GUILayout.EndHorizontal();

            if (setting != null)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                for (int i = 0; i < setting.Length; i++)
                {
                    var s           = setting[i];
                    var settingPath = settingPaths[i];

                    // 設定が削除されている場合などに対応
                    if (s == null)
                    {
                        continue;
                    }

                    // 検索ワードチェック
                    if (!string.IsNullOrEmpty(searchTxt))
                    {
                        if (s.tableGenerate)
                        {
                            if (!searchTxt.IsSubsequence(s.tableAssetName.ToLower()))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (!searchTxt.IsSubsequence(s.className.ToLower()))
                            {
                                continue;
                            }
                        }
                    }

                    GUILayout.BeginHorizontal("box");

                    if (s.tableGenerate)
                    {
                        if (GUILayout.Button(s.tableAssetName, "Label"))
                        {
                            EditorGUIUtility.PingObject(parentSettings[i].GetInstanceID());
                            GUIUtility.ExitGUI();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(s.className, "Label"))
                        {
                            EditorGUIUtility.PingObject(parentSettings[i].GetInstanceID());
                            GUIUtility.ExitGUI();
                        }
                    }

                    if (s.useGSPlugin)
                    {
                        if (GUILayout.Button("DownLoad", GUILayout.Width(110)))
                        {
                            GSPlugin.GSPluginSettings.Sheet sheet = new GSPlugin.GSPluginSettings.Sheet();
                            sheet.sheetId = s.sheetID;
                            sheet.gid     = s.gid;

                            string absolutePath = CCLogic.GetFilePathRelativesToAssets(settingPath, s.csvFilePath);

                            // 先頭の Assets を削除する
                            if (absolutePath.StartsWith("Assets/"))
                            {
                                sheet.targetPath = absolutePath.Substring(6);
                            }
                            else
                            {
                                Debug.LogError("unexpected downloadPath: " + absolutePath);
                            }

                            sheet.isCsv = true;

                            GSPlugin.GSEditorWindow.DownloadOne(sheet, settingPath);
                        }
                    }

                    GUI.enabled = s.canGenerateCode;
                    if (GUILayout.Button("Generate Code", GUILayout.Width(110)) && !isDownloading)
                    {
                        GlobalCCSettings gSettings = CCLogic.GetGlobalSettings();
                        isDownloading = true;
                        GenerateOneCode(s, gSettings, settingPath);
                        isDownloading = false;

                        GUIUtility.ExitGUI();
                    }

                    GUI.enabled = s.canCreateAsset;

                    if (GUILayout.Button("Create Assets", GUILayout.Width(110)) && !isDownloading)
                    {
                        GlobalCCSettings gSettings = CCLogic.GetGlobalSettings();
                        isDownloading = true;
                        CreateOneAssets(s, gSettings, settingPath);
                        isDownloading = false;

                        GUIUtility.ExitGUI();
                    }

                    GUI.enabled = true;
                    
                    // GS Plugin を使う場合は Open ボタンを用意する.
                    if (s.useGSPlugin)
                    {
                        if (GUILayout.Button("Open", GUILayout.Width(80)) && !isDownloading)
                        {
                            GSPlugin.GSUtils.OpenURL(s.sheetID, s.gid);
                            GUIUtility.ExitGUI();
                        }
                    }

                    {
                        string mainOutputPath = CCLogic.GetMainOutputPath(s, settingPath);

                        if (mainOutputPath != null)
                        {
                            Object outputRef = AssetDatabase.LoadAssetAtPath<Object>(mainOutputPath);

                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.ObjectField(outputRef, typeof(Object), false, GUILayout.Width(100));
                            EditorGUI.EndDisabledGroup();
                        }
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.EndScrollView();

                GUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Generate All Codes", "LargeButtonMid") && !isDownloading)
                {
                    GlobalCCSettings gSettings = CCLogic.GetGlobalSettings();
                    isDownloading = true;
                    GenerateAllCode(setting, gSettings, settingPaths);
                    isDownloading = false;

                    GUIUtility.ExitGUI();
                }

                if (GUILayout.Button("Create All Assets", "LargeButtonMid") && !isDownloading)
                {
                    GlobalCCSettings gSettings = CCLogic.GetGlobalSettings();
                    isDownloading = true;
                    CreateAllAssets(setting, gSettings, settingPaths);
                    isDownloading = false;

                    GUIUtility.ExitGUI();
                }

                GUILayout.EndHorizontal();
            }
        }

        public static void GenerateAllCode(CsvConverterSettings.Setting[] setting, GlobalCCSettings gSettings,
            string[]                                                      settingPaths)
        {
            int i = 0;

            try
            {
                foreach (CsvConverterSettings.Setting s in setting)
                {
                    show_progress(s.className, (float) i / setting.Length, i, setting.Length);
                    CsvConvert.GenerateCode(s, gSettings, settingPaths[i]);
                    i++;
                    show_progress(s.className, (float) i / setting.Length, i, setting.Length);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        public static void CreateAllAssets(CsvConverterSettings.Setting[] setting, GlobalCCSettings gSettings,
            string[]                                                      settingPaths)
        {
            try
            {
                for (int i = 0; i < setting.Length; i++)
                {
                    CsvConvert.CreateAssets(setting[i], gSettings, settingPaths[i]);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        public static void GenerateOneCode(CsvConverterSettings.Setting s, GlobalCCSettings gSettings,
            string                                                      settingPath)
        {
            show_progress(s.className, 0, 0, 1);

            try
            {
                CsvConvert.GenerateCode(s, gSettings, settingPath);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            show_progress(s.className, 1, 1, 1);

            EditorUtility.ClearProgressBar();
        }

        public static void CreateOneAssets(CsvConverterSettings.Setting s, GlobalCCSettings gSettings,
            string                                                      settingPath)
        {
            try
            {
                CsvConvert.CreateAssets(s, gSettings, settingPath);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        private static void show_progress(string name, float progress, int i, int total)
        {
            EditorUtility.DisplayProgressBar("Progress", progress_msg(name, i, total), progress);
        }

        private static string progress_msg(string name, int i, int total)
        {
            return string.Format("Creating {0} ({1}/{2})", name, i, total);
        }

        private static string SearchField(string text)
        {
            Rect rect = GUILayoutUtility.GetRect(16f, 24f, 16f, 24f, new GUILayoutOption[]
            {
                GUILayout.Width(400f), // 検索ボックスのサイズ
            });
            rect.x += 4f;
            rect.y += 4f;

            return (string) ToolbarSearchField(rect, text);
        }

        private static string ToolbarSearchField(Rect position, string text)
        {
            Rect rect = position;
            rect.x     += position.width;
            rect.width =  14f;

            if (toolbarSearchField == null)
            {
                toolbarSearchField = GetStyle("ToolbarSeachTextField");
            }

            text = EditorGUI.TextField(position, text, toolbarSearchField);
            if (text == "")
            {
                if (toolbarSearchFieldCancelButtonEmpty == null)
                {
                    toolbarSearchFieldCancelButtonEmpty = GetStyle("ToolbarSeachCancelButtonEmpty");
                }

                GUI.Button(rect, GUIContent.none, toolbarSearchFieldCancelButtonEmpty);
            }
            else
            {
                if (toolbarSearchFieldCancelButton == null)
                {
                    toolbarSearchFieldCancelButton = GetStyle("ToolbarSeachCancelButton");
                }

                if (GUI.Button(rect, GUIContent.none, toolbarSearchFieldCancelButton))
                {
                    text                       = "";
                    GUIUtility.keyboardControl = 0;
                }
            }

            return text;
        }

        private static GUIStyle GetStyle(string styleName)
        {
            GUIStyle gUIStyle = GUI.skin.FindStyle(styleName);
            if (gUIStyle == null)
            {
                gUIStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
            }

            if (gUIStyle == null)
            {
                Debug.LogError("Missing built-in guistyle " + styleName);
                gUIStyle = new GUIStyle();
            }

            return gUIStyle;
        }
    }
}