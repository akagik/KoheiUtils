namespace KoheiUtils
{
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Text))]
    public class LocalizedText : AbstractLocalizedText
    {
        private Text _textUI;

        public Text textUI
        {
            get
            {
                if (_textUI == null)
                {
                    _textUI = GetComponent<Text>();
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
}
