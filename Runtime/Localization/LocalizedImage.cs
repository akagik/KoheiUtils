namespace KoheiUtils
{
    using UnityEngine;
    using UnityEngine.UI;

    [RequireComponent(typeof(Image))]
    public class LocalizedImage : AbstractLocalizedSprite
    {
        private Image _image;

        private Image image
        {
            get
            {
                if (_image == null)
                {
                    _image = GetComponent<Image>();
                }

                return _image;
            }
        }

        public override Sprite sprite => image.sprite;

        public override void UpdateSprite()
        {
            image.sprite = LocalizationManager.Instance.GetSprite(key);
        }
    }
}
