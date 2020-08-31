using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

[NodeEditor.CustomNodeEditorAttribute(typeof(TransitionNode))]
public class TransitionNodeEditor : NodeEditor
{
    private bool zoomed = false;
    
    public override void OnBodyGUI()
    {
        EditorGUIUtility.labelWidth = 120f;
        base.OnBodyGUI();
    }

    public override int GetWidth()
    {
        
        TransitionNode asTransitionNode = target as TransitionNode;
        if (asTransitionNode != null)
            zoomed = asTransitionNode.Zoom;
        
        return zoomed ? 225 : 400;
    }
    
    public override Color GetTint()
    {
        Color tint = new Color(80f/255f, 46f/255f, 50f/255f, 1f);
        return tint;
    }
}
