using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using KoheiUtils.GSPlugin.v4;
using UnityEngine.Networking;

namespace KoheiUtils {
    // スプレッドシートをウェブから引っ張ってきて CsvData に変換する.
    // エントリポイントは LoadGS.
    public class GSLoader {
        public static readonly string WORKSHEET_LIST_URL = "https://spreadsheets.google.com/feeds/worksheets/{0}/public/full?alt=json";

        List<GSWorksheet> _worksheets;
        
        // 結果受け取り
        public bool isSuccess { get; private set; }
        public CsvData loadedCsvData { get; private set; }

        public GSLoader()
        {
            isSuccess = false;
            loadedCsvData = null;
        }

        public static string ExtractGID(string url) {
            Uri uri = new Uri(url);
            string query = uri.Query;
            string gid = "";

            int firstIndex = query.IndexOf("gid=", StringComparison.OrdinalIgnoreCase) + 4;

            string leftovers = query.Substring(firstIndex);

            int len = leftovers.IndexOf("&", StringComparison.OrdinalIgnoreCase) + 1;

            if (len == 0) {
                gid = query.Substring(firstIndex);
            }
            else {
                gid = query.Substring(firstIndex, len - 1);
            }

            return gid;
        }

        /// <summary>
        /// 指定の sheetId, gid のシートを CsvData としてダウンロードする.
        /// 
        /// Google Spreadsheet URL に2つのリクエストを送る。
        /// まず一つ目は sheetId に対する複数の worksheet を持つ URL へのリクエスト。
        /// 二つ目は その sheetId の中で特定の gid を持つ URL へのリクエスト。
        /// </summary>
        public IEnumerator LoadGS(string sheetId, string gid, string apiKey = "", bool useV4 = true)
        {
            isSuccess = false;

            if (useV4)
            {
                var coroutine = GSLoaderV4.LoadCsvData(sheetId, gid, apiKey);
                yield return coroutine;

                loadedCsvData = GSLoaderV4.csvData;
                isSuccess = loadedCsvData != null;
            }
            // V3 old api
            else
            {
                // Load spread sheet
                yield return EditorCoroutineRunner.StartCoroutine(downloadWorksheets(sheetId));

                if (_worksheets == null)
                {
                    Debug.LogError("Failed to download worksheets");
                    yield break;
                }

                // Load each worksheet
                foreach (GSWorksheet sheet in _worksheets) {
                    if (sheet.gid == gid) {
                        yield return EditorCoroutineRunner.StartCoroutine(LoadCsvData(sheet));
                        yield break;
                    }
                }
            }
        }

        public IEnumerator LoadCsvData(GSWorksheet sheet) {
            string title = sheet.title;
            string url = sheet.link;

            UnityWebRequest req = UnityWebRequest.Get(url);

            var op = req.SendWebRequest();
            
            while (!op.isDone)
            {
                yield return null;
            }

            if (req.isNetworkError || req.isHttpError) {
                Debug.Log(req.error);
                yield break;
            }
            else if (req.responseCode != 200) {
                yield break;
            }

            string resJson = req.downloadHandler.text;

            resJson = resJson.Replace("$", "_d_");
            GSResponse response = JsonUtility.FromJson<GSResponse>(resJson);

            if (response == null || response.feed == null) {
                yield break;
            }

            List<List<string>> cellList = new List<List<string>>();

            foreach (GSResponse.Entry entry in response.feed.entry) {
                var cell = entry.gs_d_cell;

                int row = cell.row;
                int col = cell.col;
                string inputValue = cell._d_t;

                while (cellList.Count < row)
                    cellList.Add(new List<string>());

                while (cellList[row - 1].Count < col)
                    cellList[row - 1].Add(string.Empty);

                cellList[row - 1][col - 1] = inputValue;
            }

            CsvData csv = ScriptableObject.CreateInstance<CsvData>();
            csv.SetFromList(cellList);

            this.loadedCsvData = csv;
            this.isSuccess = true;
        }
        
        IEnumerator downloadWorksheets(string sheetId) {
            string url_worksheet = string.Format(WORKSHEET_LIST_URL, sheetId);
            UnityWebRequest req = UnityWebRequest.Get(url_worksheet);

            var op = req.SendWebRequest();

            while (!op.isDone)
            {
                yield return null;
            }
            
            if (req.isNetworkError || req.isHttpError) {
                Debug.Log(req.error);
                yield break;
            }
            else if (req.responseCode != 200) {
                yield break;
            }

            string workSheetText = req.downloadHandler.text;
            workSheetText = workSheetText.Replace("$", "_d_");
            GSResponse response = JsonUtility.FromJson<GSResponse>(workSheetText);

            if (response == null || response.feed == null) {
                yield break;
            }

            List<GSWorksheet> worksheets = new List<GSWorksheet>();

            foreach (GSResponse.Entry entry in response.feed.entry) {
                string worksheetTitle = entry.title._d_t;

                GSWorksheet worksheet = new GSWorksheet(worksheetTitle);
                bool gid_check = false;
                bool cells_check = false;

                foreach (GSResponse.Link link in entry.link) {
                    string url = link.href;

                    if (url.Contains("/cells/")) {
                        worksheet.link = url + "?alt=json";
                        cells_check = true;
                    }

                    if (url.Contains("gid=")) {
                        worksheet.gid = ExtractGID(url);
                        gid_check = true;
                    }

                    if (cells_check && gid_check) {
                        break;
                    }
                }
                worksheets.Add(worksheet);
            }
            _worksheets = worksheets;
        }

        public class GSWorksheet {
            public string title;
            public string link;
            public string gid;

            public GSWorksheet(string title) {
                this.title = title;
            }
        }
    }
}
