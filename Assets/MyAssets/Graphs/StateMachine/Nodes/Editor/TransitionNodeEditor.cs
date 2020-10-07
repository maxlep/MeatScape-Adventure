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
        
        //End the current GUI Area that is restricted to node's dimensions
        GUILayout.EndArea();
        
        Vector2 nodePos = NodeEditorWindow.current.GridToWindowPositionNoClipped(target.position);
        
        //Show Condition Preview below if collapsed
        if (zoomed)
        {
            
            Vector2 nodeLabelPos = NodeEditorWindow.current.GridToWindowPositionNoClipped(target.position + 
                                                                                          new Vector2(17f, 130f));
            Vector2 topLeftPos = NodeEditorWindow.current.GridToWindowPositionNoClipped(target.position + 
                                                                                     new Vector2(0f, 110));
            
            //Borders
            EditorGUI.DrawRect(new Rect(topLeftPos+ new Vector2(5f, 0f), new Vector2(GetWidth() - 10, 300f)),
                new Color(80f/255f, 46f/255f, 50f/255f, 1f));
            
            //Text Area
            EditorGUI.DrawRect(new Rect(topLeftPos + new Vector2(12f, 0f), new Vector2(GetWidth() - 25, 290f)),
                new Color(0, 0, 0, .9f));
            
            //Condition Preview
            TransitionNode nodeAsTransition = target as TransitionNode;
            GUIStyle labelStyle = new GUIStyle();
            
            float minFontSize = 20f;
            float maxFontSize = 45f;
            float maxZoom = 5f;
            labelStyle.fontSize = Mathf.CeilToInt(Mathf.Lerp(minFontSize, maxFontSize, NodeEditorWindow.current.zoom / maxZoom));
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.normal.textColor = new Color(1f, .85f, .85f);
            labelStyle.alignment = TextAnchor.UpperLeft;
            labelStyle.wordWrap = true;
            
            
            GUI.Label(new Rect(nodeLabelPos, new Vector2(GetWidth() - 25f, 50f)), nodeAsTransition.ConditionPreview,
                labelStyle);
        }


        //Put back the GUI area that is restricted to node's dimensions
        GUILayout.BeginArea(new Rect(nodePos, new Vector2(GetWidth(), 4000)));
    }

    public override int GetWidth()
    {
        
        TransitionNode asTransitionNode = target as TransitionNode;
        if (asTransitionNode != null)
            zoomed = asTransitionNode.Zoom;
        
        return zoomed ? 500 : 500;
    }
    
    public override Color GetTint()
    {
        Color tint = new Color(80f/255f, 46f/255f, 50f/255f, 1f);
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
