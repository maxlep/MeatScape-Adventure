using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XNodeEditor;

[NodeEditor.CustomNodeEditorAttribute(typeof(StartNode))]
public class StartNodeEditor : NodeEditor
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
        NodeEditorPreferences.Settings prefs = NodeEditorPreferences.GetSettings();
        GUIStyle labelStyle = XNodeUtils.ZoomBasedStyle(35f, 85f, NodeEditorWindow.current.zoom,
            prefs.minZoom, prefs.maxZoom,   new Color(255f/255f, 197f/255f, 156f/255f, 1f), FontStyle.Bold, TextAnchor.LowerCenter, false);
        GUI.Label(new Rect(nodeLabelPos, new Vector2(GetWidth(), 50f)), target.name,
            labelStyle);
    
        //Put back the GUI area that is restricted to node's dimensions
        Vector2 nodePos = NodeEditorWindow.current.GridToWindowPositionNoClipped(target.position);
        GUILayout.BeginArea(new Rect(nodePos, new Vector2(GetWidth(), 4000)));
    }

    public override int GetWidth()
    {
        return 300;
    }
    
    public override Color GetTint()
    {
        Color tint = new Color(128f/255f, 76f/255f, 40f/255f, 1f);
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
