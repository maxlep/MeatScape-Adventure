using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

[NodeEditor.CustomNodeEditorAttribute(typeof(StateNode))]
public class StateNodeEditor : NodeEditor
{
    private bool zoomed = false;

    public override void OnBodyGUI()
    {
        EditorGUIUtility.labelWidth = 350;
        base.OnBodyGUI();
    }

    public override int GetWidth()
    {
        StateNode asStateNode = target as StateNode;
        if (asStateNode != null)
            zoomed = asStateNode.Zoom;

        return zoomed ? 225 : 400;
    }
    
    public override Color GetTint()
    {
        Color tint;
        bool isActiveNode = (target as StateNode).isActiveState;

        if (isActiveNode)
            tint = new Color(110f/255f, 110f/255f, 60f/255f, 1f);
        else
            tint = new Color(46f/255f, 50f/255f, 80f/255f, 1f);
        
        return tint;
    }
}
