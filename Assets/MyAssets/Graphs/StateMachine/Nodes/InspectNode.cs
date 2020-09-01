using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Node = XNode.Node;

public class InspectNode : Node
{
    [ListDrawerSettings(OnEndListElementGUI = "DrawNextButton", Expanded = true, DraggableItems = false, IsReadOnly = true, HideAddButton = true, HideRemoveButton =  true)][SerializeField] [HorizontalGroup("t")]
    [VerticalGroup("t/Previous")] [LabelText("Previous")] private List<StateNode> previousStates = new List<StateNode>();
    
    [ListDrawerSettings(OnEndListElementGUI = "DrawNextButton", Expanded = true, DraggableItems = false, IsReadOnly = true, HideAddButton = true, HideRemoveButton =  true)][SerializeField] [HorizontalGroup("t")]
    [VerticalGroup("t/Next")] [LabelText("Next")] private List<StateNode> nextStates = new List<StateNode>();

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
        previousStates = previousStates.Union(inspectedState.previousStates).ToList();
        nextStates = nextStates.Union(inspectedState.nextStates).ToList();
    }
}
