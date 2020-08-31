using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

[NodeEditor.CustomNodeEditorAttribute(typeof(StateReferenceNode))]
public class StateReferenceNodeEditor : NodeEditor
{
    private bool zoomed = false;

    public override void OnBodyGUI()
    {
        EditorGUIUtility.labelWidth = 350;
        base.OnBodyGUI();
    }

    public override int GetWidth()
    {
        return 300;
    }
    
    public override Color GetTint()
    {
        Color tint;

        tint = new Color(46f/255f, 80f/255f, 50f/255f, 1f);
        
        return tint;
    }
}