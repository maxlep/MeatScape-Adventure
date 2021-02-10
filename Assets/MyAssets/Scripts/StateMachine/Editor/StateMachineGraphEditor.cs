using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XNode;
using XNodeEditor;

[CustomNodeGraphEditor(typeof(StateMachineGraph))]
public class StateMachineGraphEditor : NodeGraphEditor
{
    /// <summary> Add items for the context menu when right-clicking this node. Override to add custom menu items. </summary>
    public override void AddContextMenuItems(GenericMenu menu)
    {
        menu.AddItem(new GUIContent("Init State Machines"), false,
            () => (target as StateMachineGraph).parentMachine.InitStateMachines(false));
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Expand All"), false, () => (target as StateMachineGraph).ToggleExpandAll(false));
        menu.AddItem(new GUIContent("Collapse All"), false, () => (target as StateMachineGraph).ToggleExpandAll(true));
        menu.AddSeparator("");
        Vector2 pos = NodeEditorWindow.current.WindowToGridPosition(Event.current.mousePosition);
        var nodeTypes = NodeEditorReflection.nodeTypes.OrderBy(type => GetNodeMenuOrder(type)).ToArray();
        for (int i = 0; i < nodeTypes.Length; i++) {
            Type type = nodeTypes[i];

            //Get node context menu path
            string path = GetNodeMenuName(type);
            if (string.IsNullOrEmpty(path)) continue;

            // Check if user is allowed to add more of given node type
            XNode.Node.DisallowMultipleNodesAttribute disallowAttrib;
            bool disallowed = false;
            if (NodeEditorUtilities.GetAttrib(type, out disallowAttrib)) {
                int typeCount = target.nodes.Count(x => x.GetType() == type);
                if (typeCount >= disallowAttrib.max) disallowed = true;
            }

            // Add node entry to context menu
            if (disallowed) menu.AddItem(new GUIContent(path), false, null);
            else menu.AddItem(new GUIContent(path), false, () => {
                XNode.Node node = CreateNode(type, pos);
                NodeEditorWindow.current.AutoConnect(node);
            });
        }
        menu.AddSeparator("");
        if (NodeEditorWindow.copyBuffer != null && NodeEditorWindow.copyBuffer.Length > 0) menu.AddItem(new GUIContent("Paste"), false, () => NodeEditorWindow.current.PasteNodes(pos));
        else menu.AddDisabledItem(new GUIContent("Paste"));
        menu.AddItem(new GUIContent("Preferences"), false, () => NodeEditorReflection.OpenPreferences());
        menu.AddCustomContextMenuItems(target);
    }
    
}
