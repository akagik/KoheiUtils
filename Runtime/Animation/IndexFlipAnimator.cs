namespace KoheiUtils
{
    /// <summary>
    /// index によるアニメーションmap を持った FlipAnimator.
    /// </summary>
    public class IndexFlipAnimator : FlipAnimatorBase
    {
        [Sirenix.OdinInspector.PropertySpace(0, 10f)]
        public FlipAnimationController controller;

        public override FlipAnimInfo GetFlipAnimInfo(int key)
        {
            if (key < 0 || key >= controller.animations.Length)
            {
                return null;
            }

            return controller.animations[key];
        }

        public override bool HasAnimation(int index)
        {
            return 0 <= index && index < controller.animations.Length;
        }
    }
}