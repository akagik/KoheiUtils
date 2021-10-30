using System.Collections;

#if ODIN_INSPECTOR
using Sirenix.Utilities;
#endif

namespace KoheiUtils
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEditor;
    using System.IO;
    using System.Linq;
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
        ConvertSetting[] cachedAllSettings;

        [MenuItem("Window/CsvConverter", false, 0)]
        static public void OpenWindow()
        {
            EditorWindow.GetWindow<CsvConverterWindow>(false, "CsvConverter", true).Show();
        }

        void OnFocus()
        {
            // isAll用のデータをキャッシュ
            var allSettingList       = new List<ConvertSetting>();

            string[] settingGUIDArray = AssetDatabase.FindAssets("t:ConvertSetting");
            for (int i = 0; i < settingGUIDArray.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(settingGUIDArray[i]);
                var    settings  = AssetDatabase.LoadAssetAtPath<ConvertSetting>(assetPath);
                allSettingList.Add(settings);
            }

            cachedAllSettings       = allSettingList.ToArray();
        }

        private void OnGUI()
        {
            GUILayout.Space(6f);
            ConvertSetting[] settings = null;

            if (cachedAllSettings != null)
            {
                settings = cachedAllSettings;
            }

            // 検索ボックスを表示
            GUILayout.BeginHorizontal();
            searchTxt = SearchField(searchTxt);
            searchTxt = searchTxt.ToLower();
            GUILayout.EndHorizontal();

            if (settings != null)
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition);

                for (int i = 0; i < settings.Length; i++)
                {
                    var s = settings[i];

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

#if ODIN_INSPECTOR
                    // ------------------------------
                    // 設定を複製ボタン.
                    // ------------------------------
                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        // var copied = Scripta
                        // var copied = new ConvertSetting(s);
                        // newList[newList.Length - 1] = copied;
                        // parent.list                 = newList;
                        //
                        // var window = CCSettingsEditWindow.OpenWindow();
                        // window.SetSettings(copied, parent);
                        //
                        // GUIUtility.ExitGUI();
                    }

                    // ------------------------------
                    // 設定を編集ボタン.
                    // ------------------------------
                    var edit = EditorGUIUtility.Load("editicon.sml") as Texture2D;
                    if (GUILayout.Button(edit, GUILayout.Width(20)))
                    {
                        var window = CCSettingsEditWindow.OpenWindow();
                        window.SetSettings(s);
                        GUIUtility.ExitGUI();
                    }
#endif

                    // ------------------------------
                    // テーブル名 (enum の場合は enum名) を表示.
                    // クリックして、設定ファイルに飛べるようにする.
                    // ------------------------------
                    if (s.tableGenerate)
                    {
                        if (GUILayout.Button(s.tableAssetName, "Label"))
                        {
                            EditorGUIUtility.PingObject(s.GetInstanceID());
                            GUIUtility.ExitGUI();
                        }
                    }
                    else
                    {
                        if (GUILayout.Button(s.className, "Label"))
                        {
                            EditorGUIUtility.PingObject(s.GetInstanceID());
                            GUIUtility.ExitGUI();
                        }
                    }

                    // ------------------------------
                    // GS Plugin 使う場合のボタン.
                    // 
                    // Import ボタン
                    // Open ボタン
                    // ------------------------------
                    if (s.useGSPlugin)
                    {
                        if (GUILayout.Button("Import", GUILayout.Width(110)))
                        {
                            EditorCoroutineRunner.StartCoroutine(ExecuteImport(s));
                            GUIUtility.ExitGUI();
                        }

                        // GS Plugin を使う場合は Open ボタンを用意する.
                        if (s.useGSPlugin)
                        {
                            if (GUILayout.Button("Open", GUILayout.Width(80)) && !isDownloading)
                            {
                                GSUtils.OpenURL(s.sheetID, s.gid);
                                GUIUtility.ExitGUI();
                            }
                        }


                        if (s.verboseBtn)
                        {
                            if (GUILayout.Button("DownLoad", GUILayout.Width(110)))
                            {
                                EditorCoroutineRunner.StartCoroutine(ExecuteDownload(s));
                                GUIUtility.ExitGUI();
                            }
                        }
                    }

                    // ------------------------------
                    // コード生成ボタン.
                    // v0.1.2 からは Import に置き換え.
                    // ------------------------------
                    if (s.verboseBtn)
                    {
                        GUI.enabled = s.canGenerateCode;
                        if (GUILayout.Button("Generate Code", GUILayout.Width(110)) && !isDownloading)
                        {
                            GlobalCCSettings gSettings = CCLogic.GetGlobalSettings();
                            isDownloading = true;
                            GenerateOneCode(s, gSettings);
                            isDownloading = false;

                            GUIUtility.ExitGUI();
                        }
                    }

                    // ------------------------------
                    // アセット生成ボタン.
                    // v0.1.2 からは Import に置き換え.
                    // ------------------------------
                    if (s.verboseBtn)
                    {
                        GUI.enabled = s.canCreateAsset;

                        if (GUILayout.Button("Create Assets", GUILayout.Width(110)) && !isDownloading)
                        {
                            CreateAssetsJob createAssetsJob = new CreateAssetsJob(s);
                            createAssetsJob.Execute();
                            GUIUtility.ExitGUI();
                        }
                    }

                    GUI.enabled = true;

                    // ------------------------------
                    // 成果物参照まど.
                    // ------------------------------
                    {
                        Object outputRef = null;
                        
                        if (s.join)
                        {
                            outputRef = s.targetTable;
                        }
                        else
                        {
                            string mainOutputPath = CCLogic.GetMainOutputPath(s);

                            if (mainOutputPath != null)
                            {
                                outputRef = AssetDatabase.LoadAssetAtPath<Object>(mainOutputPath);
                            }
                        }
                        
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField(outputRef, typeof(Object), false, GUILayout.Width(100));
                        EditorGUI.EndDisabledGroup();
                    }

                    GUILayout.EndHorizontal();
                }

                GUILayout.EndScrollView();

                GUILayout.BeginHorizontal("box");
                if (GUILayout.Button("Generate All Codes", "LargeButtonMid") && !isDownloading)
                {
                    GlobalCCSettings gSettings = CCLogic.GetGlobalSettings();
                    isDownloading = true;
                    GenerateAllCode(settings, gSettings);
                    isDownloading = false;

                    GUIUtility.ExitGUI();
                }

                if (GUILayout.Button("Create All Assets", "LargeButtonMid") && !isDownloading)
                {
                    GlobalCCSettings gSettings = CCLogic.GetGlobalSettings();
                    isDownloading = true;
                    CreateAllAssets(settings, gSettings);
                    isDownloading = false;

                    GUIUtility.ExitGUI();
                }

                GUILayout.EndHorizontal();
            }
        }

        static bool downloadSuccess;

        public static IEnumerator ExecuteImport(ConvertSetting s)
        {
            downloadSuccess = false;
            yield return EditorCoroutineRunner.StartCoroutine(ExecuteDownload(s));

            if (!downloadSuccess)
            {
                yield break;
            }
            
            CreateAssetsJob createAssetsJob = new CreateAssetsJob(s);

            // Generate Code if type script is not found.
            Type assetType;
            if (s.isEnum || !CsvConvert.TryGetTypeWithError(s.className, out assetType,
                s.checkFullyQualifiedName, dialog: false))
            {
                GlobalCCSettings gSettings = CCLogic.GetGlobalSettings();
                GenerateOneCode(s, gSettings);

                if (!s.isEnum)
                {
                    EditorUtility.DisplayDialog(
                        "Code Generated",
                        "Please reimport for creating assets after compiling",
                        "ok"
                    );
                }
            }
            // Create Assets
            else
            {
                createAssetsJob.Execute();
            }
        }
        
        public static IEnumerator ExecuteDownload(ConvertSetting s)
        {
            GSPluginSettings.Sheet sheet = new GSPluginSettings.Sheet();
            sheet.sheetId = s.sheetID;
            sheet.gid     = s.gid;

            GlobalCCSettings gSettings = CCLogic.GetGlobalSettings();

            string csvPath = s.GetCsvPath(gSettings);
            if (string.IsNullOrWhiteSpace(csvPath))
            {
                Debug.LogError("unexpected downloadPath: " + csvPath);
                downloadSuccess = false;
                yield break;
            }

            string absolutePath = CCLogic.GetFilePathRelativesToAssets(s.GetDirectoryPath(), csvPath);

            // 先頭の Assets を削除する
            if (absolutePath.StartsWith("Assets" + Path.DirectorySeparatorChar))
            {
                sheet.targetPath = absolutePath.Substring(6);
            }
            else
            {
                Debug.LogError("unexpected downloadPath: " + absolutePath);
                downloadSuccess = false;
                yield break;
            }

            sheet.isCsv = true;

            string title = "Google Spreadsheet Loader";
            yield return EditorCoroutineRunner.StartCoroutineWithUI(GSEditorWindow.Download(sheet, s.GetDirectoryPath()), title, true);
            
            // 成功判定を行う.
            if (GSEditorWindow.previousDownloadSuccess)
            {
                downloadSuccess = true;
            }
            
            yield break;
        }

        public static void GenerateAllCode(ConvertSetting[] setting, GlobalCCSettings gSettings)
        {
            int i = 0;

            try
            {
                foreach (ConvertSetting s in setting)
                {
                    show_progress(s.className, (float) i / setting.Length, i, setting.Length);
                    CsvConvert.GenerateCode(s, gSettings);
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

        public static void CreateAllAssets(ConvertSetting[] setting, GlobalCCSettings gSettings)
        {
            try
            {
                for (int i = 0; i < setting.Length; i++)
                {
                    CsvConvert.CreateAssets(setting[i], gSettings);
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

        public static void GenerateOneCode(ConvertSetting s, GlobalCCSettings gSettings)
        {
            show_progress(s.className, 0, 0, 1);

            try
            {
                CsvConvert.GenerateCode(s, gSettings);
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