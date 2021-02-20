namespace KoheiUtils
{
#if TextMeshPro
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using TMPro;

    [RequireComponent(typeof(TMP_Text))]
    public class TMP_LocalizedText : AbstractLocalizedText
    {
        private TMP_Text _textUI;
        public TMP_Text textUI {
            get {
                if (_textUI == null) {
                    _textUI = GetComponent<TMP_Text>();
                }
                return _textUI;
            }
        }

        public override string text
        {
            get
            {
                UpdateText();
                return textUI.text;
            }
        }

        public override void UpdateText()
        {
            textUI.text = LocalizationManager.Instance.Get(key);
        }
    }
#endif
}
