using Unity.Collections;
using UnityEngine;

#if UNITY_2019_3_OR_NEWER
using UnityEngine.Animations;
#else
using UnityEngine.Experimental.Animations;
#endif

public struct NoOpJob : IAnimationJob
{
    public void ProcessRootMotion(AnimationStream stream)
    {

    }

    public void ProcessAnimation(AnimationStream stream)
    {

    }
}
