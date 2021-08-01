using System.Collections;
using System.Collections.Generic;
using MyAssets.Scripts.Utils;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityRigidbody;
using Den.Tools;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Events;
using Timer = System.Timers.Timer;

[TaskCategory("Actions")]
[TaskDescription("Extends base wait by playing MMFeedbacks for the duration of the action.")]
public class WaitWithFeedback : Wait
{
    public SharedTransform FeedbacksTransform;
    private MMFeedbacks _feedbacks;
    
    public override void OnStart()
    {
        base.OnStart();
        
        _feedbacks = FeedbacksTransform.Value.GetComponent<MMFeedbacks>();
        _feedbacks.PlayFeedbacks();
    }
    
    public override void OnEnd()
    {
        base.OnEnd();
        
        _feedbacks.StopFeedbacks();
    }
}
