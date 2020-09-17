using System;
using System.Collections;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using XNode;

public class StateReferenceNode : Node
{
    [Input(typeConstraint = TypeConstraint.Strict)] [PropertyOrder(-3)] public StateMachineConnection previousState;
    [Output(typeConstraint = TypeConstraint.Strict)] [PropertyOrder(-2)]  public StateMachineConnection transitions;


    [ValueDropdown("GetStateNodes")]
    [SerializeField] [Required] [HideLabel] private StateNode referencedState;
    
    protected bool isInitialized = false;
    private StateMachineGraph stateMachineGraph;

    public StateNode ReferencedState => referencedState;
    
    public bool IsInitialized
    {
        get => isInitialized;
        set => isInitialized = value;
    }

    public virtual void Initialize(StateMachineGraph parentGraph)
    {
        this.stateMachineGraph = parentGraph;
        isInitialized = true;
        PopulateLinkedNodes();
    }
    
    public virtual void RuntimeInitialize()
    {
        
    }
    
    private  List<StateNode> GetStateNodes()
    {
        return stateMachineGraph.stateNodes;
    }

    private void PopulateLinkedNodes()
    {
        linkedNodes.Clear();
        
        //Store link to referenced state
        if (referencedState != null)
        {
            linkedNodes.Add(referencedState);
        }
        
    }
    
    private void OnValidate()
    {
        name = (referencedState != null) ? $"{referencedState.GetName()} <Reference>" : "<Missing State> <Reference>";
    }


}
