namespace KoheiUtils
{
    using UnityEngine;

    /// <summary>
    /// 連番アニメーションを管理するクラス.
    /// 連番アニメーションは単純に Sprite の配列と等価.
    /// </summary>
    [CreateAssetMenu(menuName = "KoheiUtils/FlipAnimInfo")]
    public class FlipAnimInfo : ScriptableObject
    {
        public Sprite[] sprites;
        public FlipAnimationEventTrigger[] triggers;
        public float secPerFrame = 0.1f;
    }
}
