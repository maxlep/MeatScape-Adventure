using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

[NodeEditor.CustomNodeEditorAttribute(typeof(TransitionNode))]
public class TransitionNodeEditor : NodeEditor
{
    
    public override void OnBodyGUI()
    {
        EditorGUIUtility.labelWidth = 120;
        base.OnBodyGUI();
    }

    public override int GetWidth()
    {
        return 350;
    }
    
    public override Color GetTint()
    {
        Color tint = new Color(80f/255f, 46f/255f, 50f/255f, 1f);
        return tint;
    }
}
