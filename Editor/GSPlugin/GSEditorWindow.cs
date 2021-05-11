using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace KoheiUtils {
    public class GSEditorWindow : EditorWindow {
        public static string SETTINGS_KEY = "GSPlugin/settings";

        public GSPluginSettings settings;
        public string settingPath;
        public string settingDir;
        
        private bool isDownloading;
        private Vector2 scrollPosition;

        // 二重に保存しないようにするために導入
        private string savedGUID;

        public static bool previousDownloadSuccess;

        [MenuItem("Window/GSPlugin", false, 0)]
        static public void OpenWindow() {
            EditorWindow.GetWindow<GSEditorWindow>(false, "GSPlugin", true).Show();
        }

        void OnEnable() {
            string guid = EditorUserSettings.GetConfigValue(SETTINGS_KEY);
            settingPath = AssetDatabase.GUIDToAssetPath(guid);

            if (settingPath != "") {
                // Debug.Log("Found prefs settings GUID: " + EditorPrefs.GetString(SETTINGS_KEY));
                settings = AssetDatabase.LoadAssetAtPath<GSPluginSettings>(settingPath);
                // string[] settingGUIDArray = AssetDatabase.FindAssets("t:GSPluginSettings");

                settingDir = Path.GetDirectoryName(settingPath);
            }
            else {
                // Debug.Log("Not Found GUID");
            }

            // foreach (string guid in settingGUIDArray) {
            //     string path = AssetDatabase.GUIDToAssetPath(guid);
            //     settings = AssetDatabase.LoadAssetAtPath<GSPluginSettings>(path);
            // }
        }

        void OnGUI() {
            settings = EditorGUILayout.ObjectField("Settings", settings, typeof(GSPluginSettings), false) as GSPluginSettings;

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            if (settings != null) {
                // セットされている settings 情報を EditorUserSettings に保存する.
                {
                    string guid;
                    long localId;

                    if(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(settings, out guid, out localId)) {
                        if (savedGUID != guid) {
                            // Debug.Log("Save GUID(" + guid + ") at " + SETTINGS_KEY);
                            EditorPrefs.SetString(SETTINGS_KEY, guid);
                            EditorUserSettings.SetConfigValue(SETTINGS_KEY, guid);
                            savedGUID = guid;
                        }
                    }
                }

                for (int i = 0; i < settings.sheets.Length; i++) {
                    var sheet = settings.sheets[i];

                    GUILayout.BeginHorizontal("box");
                    GUILayout.Label(sheet.targetPath);

                    if (GUILayout.Button("Download", GUILayout.Width(80)) && !isDownloading) {
                        isDownloading = true;
                        DownloadOne(sheet, settingDir);
                        isDownloading = false;

                        GUIUtility.ExitGUI();
                    }
                    
                    if (GUILayout.Button("Open", GUILayout.Width(80)) && !isDownloading) {
                        GSUtils.OpenURL(sheet.sheetId, sheet.gid);
                        GUIUtility.ExitGUI();
                    }

                    GUILayout.EndHorizontal();
                }

                if (GUILayout.Button("DownloadAll", "LargeButtonMid") && !isDownloading) {
                    isDownloading = true;

                    var sheets = new List<GSPluginSettings.Sheet>(settings.sheets);
                    DownloadAll(sheets, settingDir);
                    isDownloading = false;

                    GUIUtility.ExitGUI();
                }
            }

            GUILayout.EndScrollView();
        }

        private void DownloadAll(List<GSPluginSettings.Sheet> sheets, string settingDir) {
            string title = "Downloading All";
            
            EditorCoroutineRunner.StartCoroutineWithUI(downloadAll(sheets, settingDir), title, true);
        }
        
        private static IEnumerator downloadAll(List<GSPluginSettings.Sheet> sheets, string settingDir) {
            float progress = 0f;
            int i = 0;

            foreach (var ss in sheets) {
                EditorCoroutineRunner.UpdateUILabel("Downloading " + ss.targetPath);
                yield return EditorCoroutineRunner.StartCoroutine(Download(ss, settingDir));
                progress = (float)(++i) / sheets.Count;
                EditorCoroutineRunner.UpdateUIProgressBar(progress);
            }

            AssetDatabase.Refresh();
        }

        public static void DownloadOne(GSPluginSettings.Sheet sheet, string settingPath)
        {
            string title = "Google Spreadsheet Loader";
            
            EditorCoroutineRunner.StartCoroutineWithUI(Download(sheet, settingPath), title, true);
        }

        public static IEnumerator Download(GSPluginSettings.Sheet ss, string settingDir)
        {
            previousDownloadSuccess = false;
            string sheetId = ss.sheetId;
            string gid = ss.gid;
            
            string label = "Downloading " + ss.targetPath;
            EditorCoroutineRunner.UpdateUILabel(label);

            var gsLoader = new GSLoader();
            yield return EditorCoroutineRunner.StartCoroutine(gsLoader.LoadGS(sheetId, gid));

            if (!gsLoader.isSuccess)
            {
                Debug.Log("Failed to load spreadsheet data.");
                yield break;
            }
            
            CsvData csvData = gsLoader.loadedCsvData;

            string targetPathRelativeToAssets = ss.GetFilePathRelativesToAssets(settingDir);
            
            if (csvData != null) {
                if (ss.isCsv)
                {
                    string targetDir = Path.GetDirectoryName(targetPathRelativeToAssets);
                    
                    if (!Directory.Exists(targetDir))
                    {
                        try
                        {
                            Directory.CreateDirectory(targetDir);
                            Debug.Log("指定のフォルダが存在しないため、作成しました: " + targetDir);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("指定のフォルダの作成に失敗: " + e.Message);
                        }
//                            Debug.LogError("指定のフォルダは存在しません: " + targetDir);
//                        return;
                    }

                    using (var s = new StreamWriter(targetPathRelativeToAssets)) {
                        s.Write(csvData.ToString());
                    }
                }
                else {
                    AssetDatabase.CreateAsset(csvData, targetPathRelativeToAssets);
                }
                Debug.Log("Write " + ss.targetPath);
                
                previousDownloadSuccess = true;
                AssetDatabase.Refresh();
            }
            else {
                Debug.LogError("Fails for " + ss.ToString());
            }
        }

        private static string progress_msg(string path, int i, int total) {
            return string.Format("Downloading {0} ({1}/{2})", path, i, total);
        }

        //void OnInspectorUpdate()
        //{
        //    Repaint();
        //}
    }
}
