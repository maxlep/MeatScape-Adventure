using UnityEditor;
using UnityEngine;
using XNodeEditor;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes.Editor
{
    [NodeEditor.CustomNodeEditorAttribute(typeof(AnimationLayerStartNode))]
    public class AnimationLayerStartNodeEditor : StartNodeEditor
    {
        public override void OnBodyGUI()
        {
            base.OnBodyGUI();
        }

        public override int GetWidth()
        {
            return 500;
        }
        
        public override Color GetTint()
        {
            return base.GetTint();
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
}