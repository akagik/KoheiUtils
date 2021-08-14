namespace KoheiUtils
{
    using System;
    using System.IO;
    using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

    [CreateAssetMenu(menuName = "GSPlugin/GSPluginSettings")]
    public class GSPluginSettings : ScriptableObject
    {
        public Sheet[] sheets;

        [Serializable]
        public class Sheet
        {
            [Tooltip("For example, \"../tmp/test.csv\" or \"/CsvFiles/test.csv\"")]
            public string targetPath;

            [Tooltip("For example, \"1sY73oxBYzwTfon-6CGaLVG8aEqZrfN_zni1j0rizs8I\"")]
            public string sheetId;

            [Tooltip("For example, \"469520790\"")]
            public string gid;

            public bool isCsv = true;

            public override string ToString()
            {
                return $"SheetSettings(sheetId=\"{sheetId}\", gid=\"{gid}\")";
            }
            
            /// <summary>
            /// 設定ファイルのディレクトリと対象パスから Assets からのパスを求める.
            /// 対象パスが "/" から始まるかどうかで絶対パスか相対パスかを判断する.
            /// </summary>
            public string GetFilePathRelativesToAssets(string settingPath)
            {
                if (targetPath.StartsWith(Path.DirectorySeparatorChar.ToString()))
                {
                    return Path.Combine("Assets", targetPath.Substring(1));
                }

                // ".." などを解決するために一度 FullPath を取得したのち、Assets より前を削除する.
                int index = Path.GetFullPath(".").Length;
                return Path.GetFullPath(Path.Combine(settingPath, targetPath)).Substring(index + 1);
            }
        }
    }
}