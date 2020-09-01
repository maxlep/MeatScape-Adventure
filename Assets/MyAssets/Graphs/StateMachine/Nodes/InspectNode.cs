using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Node = XNode.Node;

public class InspectNode : Node
{
    [TextArea]
    [SerializeField] [HideLabel] private string text;

    public void Initialize(StateNode inspectedState)
    {
        text = inspectedState.name;
    }
}
