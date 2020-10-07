using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

[NodeEditor.CustomNodeEditorAttribute(typeof(CommentNode))]
public class CommentNodeEditor : NodeEditor
{
    public override void OnBodyGUI()
    {
        EditorGUIUtility.labelWidth = 350;
        base.OnBodyGUI();
        
        //End the current GUI Area that is restricted to node's dimensions
        GUILayout.EndArea();
        
        //Show Label Above node
        Vector2 nodeLabelPos = NodeEditorWindow.current.GridToWindowPositionNoClipped(target.position + 
                                                                                      new Vector2(0f, -60f));
        StateReferenceNode nodeAsStateRef = target as StateReferenceNode;
        GUIStyle labelStyle = new GUIStyle();
        float minFontSize = 30f;
        labelStyle.fontSize = (int) Mathf.Max( (15 * NodeEditorWindow.current.zoom), minFontSize);
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = new Color(.85f, 1f, .85f);
        labelStyle.alignment = TextAnchor.LowerCenter;

        string comment = (target as CommentNode).GetCommentText();
        GUI.Label(new Rect(nodeLabelPos, new Vector2(GetWidth(), 50f)),
            comment, labelStyle);

        //Put back the GUI area that is restricted to node's dimensions
        Vector2 nodePos = NodeEditorWindow.current.GridToWindowPositionNoClipped(target.position);
        GUILayout.BeginArea(new Rect(nodePos, new Vector2(GetWidth(), 4000)));
    }
}
