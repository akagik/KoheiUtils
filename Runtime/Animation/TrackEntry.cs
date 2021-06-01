using System;

namespace KoheiUtils
{
    public class TrackEntry
    {
        public int animationIndex;

        public bool loop;

        // アニメーションが中断されたり、完了したときに呼び出される.
        public Action onEnd;

        // アニメーションが最後まで完了したときに呼び出される.
        public Action onCompleted;
    }
}