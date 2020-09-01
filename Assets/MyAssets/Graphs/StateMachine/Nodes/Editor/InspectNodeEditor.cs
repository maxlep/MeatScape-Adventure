using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

[NodeEditor.CustomNodeEditorAttribute(typeof(InspectNode))]
public class InspectNodeEditor : NodeEditor
{

    public override void OnBodyGUI()
    {
        EditorGUIUtility.labelWidth = 350;
        base.OnBodyGUI();
    }

    public override int GetWidth()
    {
        return 550;
    }
    
    public override Color GetTint()
    {
        return new Color(50f/255f, 100f/255f, 100f/255f, 1f);
    }
}
