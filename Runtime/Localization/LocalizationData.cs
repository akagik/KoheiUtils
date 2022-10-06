namespace KoheiUtils
{
    [System.Serializable]
    public class LocalizationData
    {
        public string key;

        // SmartFormat を利用するかどうか.
        public bool smart;
        
        // NOTE: ISO 言語コードに対応.
        // https://www.asahi-net.or.jp/~ax2s-kmtn/ref/iso639.html
        public string en;
        public string ja;
        public string ko;
        public string zh_cn; // 中国語（簡体字）
        public string zh_tw; // 中国語（繁体字）
    }
}