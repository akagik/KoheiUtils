using UnityEngine;

namespace KoheiUtils
{
    public static class GSUtils
    {
        public static readonly string SHEET_URL = "https://docs.google.com/spreadsheets/d/{0}/edit#gid={1}";

        public static void OpenURL(string sheetId, string gid)
        {
            string url = string.Format(SHEET_URL, sheetId, gid);
            Application.OpenURL(url);
        }
    }
}