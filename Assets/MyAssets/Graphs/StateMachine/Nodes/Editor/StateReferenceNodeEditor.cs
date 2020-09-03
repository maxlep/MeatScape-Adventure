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
        bool isActiveNode = (target as StateReferenceNode).ReferencedState.isActiveState;

        if (isActiveNode)
            tint = new Color(110f/255f, 110f/255f, 60f/255f, 1f);
        else
        tint = new Color(46f/255f, 80f/255f, 50f/255f, 1f);
        
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
            menu.AddItem(new GUIContent("Inspect"), false, () =>
            {
                StateNode referencedState = (target as StateReferenceNode).ReferencedState;
                if (referencedState != null)
                {
                    InspectNode addedNode = referencedState.GetParentGraph().AddNode(typeof(InspectNode)) as InspectNode;
                    addedNode.position = node.position;
                    addedNode.Initialize(referencedState);
                }
                
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