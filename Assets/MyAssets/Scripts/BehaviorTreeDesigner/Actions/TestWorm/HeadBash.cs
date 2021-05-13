using System;
using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using Action = BehaviorDesigner.Runtime.Tasks.Action;

[TaskCategory("Combat")]
[TaskDescription("Attack target by moving IK Target of bone chain towards target")]
public class HeadBash : Action
{
    public SharedTransform Target;
    public SharedTransform IkTarget;
    public SharedFloat TimeToTarget;
    public LeanTweenType easingType;

    private Vector3 targetStartPosition;
    private LTDescr AttackTween;
    private bool attackComplete;

    public override void OnStart()
    {
        base.OnStart();
        attackComplete = false;
        targetStartPosition = Target.Value.position;
    }

    public override TaskStatus OnUpdate()
    {
        //Tween position towards target
        // AttackTween = LeanTween.value(gameObject, IkTarget.Value.position, targetStartPosition, TimeToTarget.Value);
        // AttackTween.setEase(easingType);
        // AttackTween.setOnUpdate((Vector3 p) =>
        // {
        //     IkTarget.Value.position = p;
        // });
        // AttackTween.setOnComplete(_ =>
        // {
        //     attackComplete = true;
        // });

        IkTarget.Value.position = targetStartPosition;


        return (attackComplete) ? TaskStatus.Success : TaskStatus.Running;
    }

    public override void OnEnd()
    {
        base.OnEnd();
        if (AttackTween != null) LeanTween.cancel(AttackTween.id);
    }
}
