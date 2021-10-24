using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace KoheiUtils.GSPlugin.v4
{
    public class GSLoaderV4
    {
        public static bool isLoadingAsync = false;
        public static CsvData csvData;
        
        [MenuItem("KoheiUtils/GSPlugins/Test")]
        public static void GSTest()
        {
            string apiKey = "AIzaSyBxhRwctKkl3BrnT5BhSrbUAxat7jv0we8";
            string spreadSheetId = "19r-698FhoVnYpUfPopfeO_CuzdpYFRrcjrMkM_M-vT4";
            string gid = "1045764977";
            _ = LoadCsvDataFromSheetAsync(spreadSheetId, gid, apiKey);
        }

        public static IEnumerator LoadCsvData(string spreadSheetId, string gid, string apiKey)
        {
            isLoadingAsync = true;
            _ = LoadCsvDataFromSheetAsync(spreadSheetId, gid, apiKey);

            while (true)
            {
                if (!isLoadingAsync)
                {
                    break;
                }
                yield return null;
            }
        }
        
        public static async Task LoadCsvDataFromSheetAsync(string spreadSheetId, string gid, string apiKey)
        {
            isLoadingAsync = true;
            GSLoaderV4.csvData = null;
            
            var title = await GetSheetsInfoAsync(spreadSheetId, gid, apiKey);

            if (title == null)
            {
                isLoadingAsync = false;
                return;
            }
            
            var csvData = await FetchAsync(spreadSheetId, title, apiKey);

            if (csvData == null)
            {
                isLoadingAsync = false;
                return;
            }

            GSLoaderV4.csvData = csvData;
            isLoadingAsync = false;
        }

        public static async Task<string> GetSheetsInfoAsync(string spreadSheetId, string gid, string apiKey)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("https://sheets.googleapis.com/v4/spreadsheets/");
            sb.Append(spreadSheetId);
            sb.Append("?");
            sb.Append("fields=sheets.properties");
            sb.Append("&key=");
            sb.Append(apiKey);

            string url = sb.ToString();

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SendWebRequest();

                while (!request.isDone)
                {
                    await Task.Delay(10);
                }

                string jsonText = request.downloadHandler.text;

                if (request.responseCode == 200)
                {
                    var json = Json.Deserialize(jsonText) as Dictionary<string, object>;

                    var list = json["sheets"] as List<object>;

                    for (int i = 0; i < list.Count; i++)
                    {
                        var properties =
                            (list[i] as Dictionary<string, object>)["properties"] as Dictionary<string, object>;

                        if (properties.TryGetValue("sheetId", out var sheetId))
                        {
                            if (properties.TryGetValue("title", out var title))
                            {
                                if (sheetId.ToString() == gid)
                                {
                                    return title.ToString();
                                }
                            }
                        }
                    }

                    // TODO return title matching gid
                    return null;
                }
                else
                {
                    Debug.LogError($"Failed to GET {url}");
                    Debug.LogError($"Error Code: {request.responseCode}");
                }
            }

            return null;
        }

        public static async Task<CsvData> FetchAsync(string spreadSheetId, string title, string apiKey)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("https://sheets.googleapis.com/v4/spreadsheets/");
            sb.Append(spreadSheetId);
            sb.Append("/values/");
            sb.Append(title);
            sb.Append("?key=");
            sb.Append(apiKey);
            sb.Append("&valueRenderOption=UNFORMATTED_VALUE");

            string url = sb.ToString();

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SendWebRequest();

                while (!request.isDone)
                {
                    await Task.Delay(10);
                }

                string jsonText = request.downloadHandler.text;

                if (request.responseCode == 200)
                {
                    var obj = Json.Deserialize(jsonText) as Dictionary<string, object>;

                    var values = obj["values"] as List<object>;

                    CsvData csv = new CsvData();
                    csv.SetFromListOfListObject(values);
                    return csv;
                }
                else
                {
                    Debug.LogError($"Failed to GET {url}");
                    Debug.LogError($"Error Code: {request.responseCode}");
                }
            }

            return null;
        }
    }
}