using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Node = XNode.Node;

[System.Serializable]
public class StateInfo
{
    [InlineButton("Test", "+")]
    [ListDrawerSettings(Expanded = true, DraggableItems = false, IsReadOnly = true, HideAddButton = true, HideRemoveButton =  true)]
    [SerializeField] [HideLabel] private StateNode state;
    [SerializeField] [HideLabel] [TextArea] private string transitionInfo;

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

    
    private List<StateNode> Test()
    {
        Debug.Log("Test");
        return null;
    }
}

public class InspectNode : Node
{
    [ListDrawerSettings(OnEndListElementGUI = "DrawNextButton", Expanded = true, DraggableItems = false, IsReadOnly = true, HideAddButton = true, HideRemoveButton =  true)][SerializeField] [HorizontalGroup("t")]
    [VerticalGroup("t/Previous")] [LabelText("Previous")] private List<StateInfo> previousStates = new List<StateInfo>();
    
    [ListDrawerSettings(OnEndListElementGUI = "DrawNextButton", Expanded = true, DraggableItems = false, IsReadOnly = true, HideAddButton = true, HideRemoveButton =  true)][SerializeField] [HorizontalGroup("t")]
    [VerticalGroup("t/Next")] [LabelText("Next")] private List<StateInfo> nextStates = new List<StateInfo>();
    
    private void DrawPreviousButton(int index)
    {
        if (SirenixEditorGUI.ToolbarButton(EditorIcons.MagnifyingGlass))
        {
            Debug.Log(this.previousStates.Count.ToString());
        }
    }
    
    private void DrawNextButton(int index)
    {
        if (SirenixEditorGUI.ToolbarButton(EditorIcons.MagnifyingGlass))
        {
            Debug.Log(this.nextStates.Count.ToString());
        }
    }
    
    public void Initialize(StateNode inspectedState)
    {
        name = $"Inspecting {inspectedState.name}";
        
        previousStates.Clear();
        nextStates.Clear();

        foreach (var previousState in inspectedState.previousStates)
        {
            StateInfo info = new StateInfo(previousState, "shiieeet");
            previousStates.Add(info);
        }
        
        foreach (var nextState in inspectedState.nextStates)
        {
            StateInfo info = new StateInfo(nextState, "shiieeet");
            nextStates.Add(info);
        }
    }
}
