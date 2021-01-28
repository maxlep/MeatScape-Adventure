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
        
        //End the current GUI Area that is restricted to node's dimensions
        GUILayout.EndArea();
        
        //Show Label Above node
        Vector2 nodeLabelPos = NodeEditorWindow.current.GridToWindowPositionNoClipped(target.position + 
                                                                                      new Vector2(0f, -60f));
        StateNode nodeAsState = target as StateNode;
        GUIStyle labelStyle = new GUIStyle();
        float minFontSize = 30f;
        labelStyle.fontSize = (int) Mathf.Max( (15 * NodeEditorWindow.current.zoom), minFontSize);
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = new Color(.85f, .85f, 1f);
        labelStyle.alignment = TextAnchor.LowerCenter;
        GUI.Label(new Rect(nodeLabelPos, new Vector2(GetWidth(), 50f)), nodeAsState.GetName(),
            labelStyle);

        //Put back the GUI area that is restricted to node's dimensions
        Vector2 nodePos = NodeEditorWindow.current.GridToWindowPositionNoClipped(target.position);
        GUILayout.BeginArea(new Rect(nodePos, new Vector2(GetWidth(), 4000)));
    }
    
    public override int GetWidth()
    {
        StateNode asStateNode = target as StateNode;
        if (asStateNode != null)
            zoomed = asStateNode.Zoom;

        return zoomed ? 500 : 500;
    }
    
    public override Color GetTint()
    {
        Color tint;
        StateNode nodeAsState = (target as StateNode);

        if (nodeAsState.isActiveState)
            tint = new Color(110f/255f, 110f/255f, 60f/255f, 1f);
        else if (nodeAsState.GetBypassState())
            tint = new Color(65f/255f, 80f/255f, 100f/255f, 1f);
        else
            tint = new Color(46f/255f, 50f/255f, 80f/255f, 1f);
        
        return tint;
    }
    
    /// <summary> Add items for the context menu when right-clicking this node. Override to add custom menu items. </summary>
    public override void AddContextMenuItems(GenericMenu menu) {
        bool canRemove = true;
        // Actions if only one node is selected
        if (Selection.objects.Length == 1 && Selection.activeObject is XNode.Node) {
            XNode.Node node = Selection.activeObject as XNode.Node;
            menu.AddItem(new GUIContent("Edit Script"), false, () =>
            {
                string assetPath = AssetDatabase.GetAssetPath(MonoScript.FromScriptableObject(target));
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<MonoScript>(assetPath));
            });
            menu.AddItem(new GUIContent("Move To Top"), false, () => NodeEditorWindow.current.MoveNodeToTop(node));
            menu.AddItem(new GUIContent("Rename"), false, NodeEditorWindow.current.RenameSelectedNode);

            canRemove = NodeGraphEditor.GetEditor(node.graph, NodeEditorWindow.current).CanRemove(node);
        }

        // Add actions to any number of selected nodes
        menu.AddItem(new GUIContent("Copy"), false, NodeEditorWindow.current.CopySelectedNodes);
        menu.AddItem(new GUIContent("Duplicate"), false, NodeEditorWindow.current.DuplicateSelectedNodes);

        if (canRemove) menu.AddItem(new GUIContent("Remove"), false, NodeEditorWindow.current.RemoveSelectedNodes);
        else menu.AddItem(new GUIContent("Remove"), false, null);

        // Custom sctions if only one node is selected
        if (Selection.objects.Length == 1 && Selection.activeObject is XNode.Node) {
            XNode.Node node = Selection.activeObject as XNode.Node;
            menu.AddCustomContextMenuItems(node);
        }
    }
}
