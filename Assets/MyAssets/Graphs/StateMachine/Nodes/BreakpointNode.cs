using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

public class BreakpointNode : PlayerStateNode
{
    [HideIf("$zoom")] [LabelWidth(120)] [TextArea] [SerializeField] private string Message = "";


    public override void Enter()
    {
        base.Enter();
        Debug.LogError(Message);
    }
}
