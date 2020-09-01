using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Node = XNode.Node;

[System.Serializable]
public class StateInfo
{
    [InlineButton("ToggleExpand", "$GetExpandButtonName")]
    [SerializeField] [HideLabel] private StateNode state;
    [SerializeField] [ShowIf("$expanded")] [HideLabel] [TextArea(3, 3)] private string transitionInfo;

    private bool expanded = false;

    public StateInfo(StateNode state, string transitionInfo)
    {
        this.state = state;
        this.transitionInfo = transitionInfo;
    }
    
    public StateNode State 
    {
        get => state;
        set => state = value;
    }
    
    public string TransitionInfo 
    {
        get => TransitionInfo;
        set => TransitionInfo = value;
    }

    public bool Expanded
    {
        get => expanded;
        set => expanded = value;
    }

    
    public void ToggleExpand()
    {
        expanded = !expanded;
    }
    
    private string GetExpandButtonName()
    {
        return expanded ? "-" : "+";
    }
}

public class InspectNode : Node
{
    [ListDrawerSettings(Expanded = true, DraggableItems = false, IsReadOnly = true, HideAddButton = true, HideRemoveButton =  true)]
    [SerializeField] [HorizontalGroup("t")] [VerticalGroup("t/Previous")] [LabelText("Previous States")]
    private List<StateInfo> previousStates = new List<StateInfo>();
    
    [ListDrawerSettings(Expanded = true, DraggableItems = false, IsReadOnly = true, HideAddButton = true, HideRemoveButton =  true)]
    [SerializeField] [HorizontalGroup("t")] [VerticalGroup("t/Next")] [LabelText("Next States")]
    private List<StateInfo> nextStates = new List<StateInfo>();

    public void Initialize(StateNode inspectedState)
    {
        name = $"Inspecting {inspectedState.name}";
        PopulateStateInfoLists(inspectedState);
    }

    private void PopulateStateInfoLists(StateNode inspectedState)
    {
        previousStates.Clear();
        nextStates.Clear();
        
        //Check if its connected to StartState
        if (inspectedState.isEntryState)
        {
            StateInfo info = new StateInfo(null, "<Start State>");
            previousStates.Add(info);
        }
            

        //Add Previous state info from transitions
        foreach (var previousTransition in inspectedState.previousTransitionNodes)
        {
            StateNode prevState = previousTransition.GetStartingState();
            string conditions = previousTransition.ConditionPreview;
            
            StateInfo info = new StateInfo(prevState, conditions);
            previousStates.Add(info);
        }

        //Add Previous state info from direct connections
        if (!inspectedState.previousNoTransitionStates.IsNullOrEmpty())
        {
            foreach (var previousNoTransState in inspectedState.previousNoTransitionStates)
            {
                StateNode prevState = previousNoTransState;
                string conditions = $"<Bypass>";
            
                StateInfo info = new StateInfo(prevState, conditions);
                previousStates.Add(info);
            }
        }
        
        //Add Next state info from transitions
        foreach (var nextTransition in inspectedState.nextTransitionNodes)
        {
            StateNode nextState = nextTransition.GetNextState();
            string conditions = nextTransition.ConditionPreview;
            
            StateInfo info = new StateInfo(nextState, conditions);
            nextStates.Add(info);
        }

        //Add Next state info from direct connection
        if (inspectedState.nextNoTransitionState != null)
        {
            StateNode nextState = inspectedState.nextNoTransitionState;
            string conditions = $"<Bypass>";
        
            StateInfo info = new StateInfo(nextState, conditions);
            nextStates.Add(info);
        }
    }
}
