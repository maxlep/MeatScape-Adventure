using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNodeEditor;

[NodeEditor.CustomNodeEditorAttribute(typeof(BreakpointNode))]
public class BreakpointNodeEditor : NodeEditor
{
    public override Color GetTint()
    {
        Color tint;
        bool isActiveNode = (target as BreakpointNode).isActiveState;

        if (isActiveNode)
            tint = new Color(110f/255f, 110f/255f, 60f/255f, 1f);
        else
            tint = new Color(120f/255f, 30f/255f, 40f/255f, 1f);
        
        return tint;
    }
}
