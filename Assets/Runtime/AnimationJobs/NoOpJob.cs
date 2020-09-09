using UnityEngine.Animations;
#if UNITY_2019_3_OR_NEWER

#else
using UnityEngine.Experimental.Animations;
#endif

namespace MyAssets.Runtime.AnimationJobs
{
    public struct NoOpJob : IAnimationJob
    {
        public void ProcessRootMotion(AnimationStream stream)
        {

        }

        public void ProcessAnimation(AnimationStream stream)
        {

        }
    }
}
