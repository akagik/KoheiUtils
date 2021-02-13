using System.IO;

namespace KoheiUtils
{
    using UnityEditor;
    using UnityEngine;

    public static class KoheiEditorTools
    {
        [MenuItem(("KoheiUtils/CaptureScreenshot"))]
        public static void CaptureScreenshot()
        {
            string tmpPath = Path.Combine(Application.dataPath, "..", "tmp");
            Debug.Log(tmpPath);
            Directory.CreateDirectory(tmpPath);
            ScreenCapture.CaptureScreenshot("tmp/screenshot.png");
        }
    }
}