using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class SubstateProcessorNode : StateNode
{
    [ListDrawerSettings(Expanded = true)]
    [HideIf("$zoom")] [LabelWidth(120)] [SerializeField] private List<StateNode> subStates;
    
    
    #region LifeCycle Methods

    public override void Enter()
    {
        base.Enter();
        subStates.ForEach(s => s.Enter());
    }

    public override void Execute()
    {
        base.Execute();
        subStates.ForEach(s => s.Execute());
    }

    public override void ExecuteFixed()
    {
        base.ExecuteFixed();
        subStates.ForEach(s => s.ExecuteFixed());
    }

    public override void Exit()
    {
        base.Exit();
        subStates.ForEach(s => s.Exit());
    }
    
    public override void DrawGizmos()
    {
        base.DrawGizmos();
        subStates.ForEach(s => s.DrawGizmos());
    }
    
    #endregion
}
