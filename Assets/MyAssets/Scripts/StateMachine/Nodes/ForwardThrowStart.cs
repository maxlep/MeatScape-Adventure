using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;

public class ForwardThrowStart : PlayerStateNode
{
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    private TriggerVariable throwClumpAnimComplete;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    private FloatValueReference ForceCancelDelay;

    private LTDescr forceCancelTween;
    
    public override void Enter()
    {
        base.Enter();
        forceCancelTween = LeanTween.value(0f, 1f, ForceCancelDelay.Value).setOnComplete(_ => throwClumpAnimComplete.Activate());
    }

    public override void Exit()
    {
        base.Exit();
        LeanTween.cancel(forceCancelTween.id);
    }
}
