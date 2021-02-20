namespace KoheiUtils
{
    using UnityEngine;
    using System.Collections.Generic;

    [CreateAssetMenu(menuName = "KoheiUtils/LocalizationTable")]
    public class LocalizationTable : ScriptableObject
    {
        public List<LocalizationData> rows;
    }
}