
namespace KoheiUtils
{
    using UnityEngine;
    using System.Runtime.CompilerServices;

    public class FlipAnimSprRenderer : FlipAnimation
    {
        [SerializeField]
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.PropertySpace(0, 10f)]
#endif
        public SpriteRenderer sprRenderer;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void SetSprite(Sprite sprite)
        {
            sprRenderer.sprite = sprite;
        }
    }
}