using UnityEngine;

namespace KoheiUtils
{
    [CreateAssetMenu(menuName = "KoheiUtils/FlipAnimationController")]
    public class FlipAnimationController : ScriptableObject
    {
        [Tooltip(("The animation at index 0 is the first default animation"))]
        public FlipAnimInfo[] animations;
    }
}