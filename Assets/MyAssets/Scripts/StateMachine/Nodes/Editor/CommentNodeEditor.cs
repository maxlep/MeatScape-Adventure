using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using XNode;
using XNodeEditor;

[NodeEditor.CustomNodeEditorAttribute(typeof(CommentNode))]
public class CommentNodeEditor : NodeEditor
{
    bool isDragging = false;
    float lastClickTime = Mathf.NegativeInfinity;
    private Vector2 lastClickPos;
    private float doubleClickTime = 1f;

    public override void OnBodyGUI()
		{
			if (Selection.objects.Length == 1 && Selection.objects[0] is XNode.Node && Selection.objects[0] == target)
				NodeEditorWindow.current.Repaint();

			if ((target as CommentNode).Minimized)
				return;

			var thisNodeRect = GUILayoutUtility.GetRect((target as CommentNode).Width, (target as CommentNode).Height);
			
			//Corner Drag rect
			var dragRect = DrawDragRect(thisNodeRect);
			
			//Draw node description
			DrawNodeDescription(thisNodeRect);

			if (Selection.objects.Length == 1 && Selection.objects[0] is XNode.Node && Selection.objects[0] == target)
			{
				Event e = Event.current;
				switch (e.type)
				{
					case EventType.MouseDrag:
						{
							if (isDragging)
							{
								(target as CommentNode).Width = Mathf.Max(200, (target as CommentNode).Width + (int)e.delta.x);
								(target as CommentNode).Height = Mathf.Max(200, (target as CommentNode).Height + (int)e.delta.y);
							}
						}
						break;

					case EventType.MouseDown:
						{
							if (thisNodeRect.Contains(e.mousePosition))
							{
								//Handle Double click to select nodes within
								if (e.mousePosition == lastClickPos &&
								    lastClickTime + doubleClickTime > Time.time)
								{
									SelectNodesInside();
								}

								lastClickTime = Time.time;
								lastClickPos = e.mousePosition;
								
								//Only check the corner for dragging
								if (dragRect.Contains(e.mousePosition))
									isDragging = true;
							}
						}
						break;

					case EventType.MouseUp:
						{
							isDragging = false;
						}
						break;
				}
			}
			else
				isDragging = false;
			
		}

		public override int GetWidth()
		{
			return (target as CommentNode).Width;
		}

		public override Color GetTint()
		{
			return (target as CommentNode).Color;
		}
		

		public override void AddContextMenuItems(GenericMenu menu)
		{
			if (Selection.objects.Length == 1 && Selection.objects[0] is XNode.Node && Selection.objects[0] == target)
			{
				menu.AddItem(new GUIContent("Send To Back"), false, SendToBack);
			}

			base.AddContextMenuItems(menu);
		}

		void SendToBack()
		{
			target.graph.nodes.Remove(this.target);
			target.graph.nodes.Insert(0, this.target);
			NodeEditorWindow.current.Repaint();
		}

		Rect DrawDragRect(Rect thisNodeRect)
		{
			int cornerDragSize = 60;
			var dragRect = thisNodeRect;
			dragRect.x += dragRect.width - cornerDragSize;
			dragRect.y += dragRect.height - cornerDragSize;
			dragRect.width = cornerDragSize;
			dragRect.height = cornerDragSize;

			Color dragRectColor = (target as CommentNode).Color;
			dragRectColor.a = .2f;
			
			EditorGUI.DrawRect(dragRect, dragRectColor);

			return dragRect;
		}

		void DrawNodeDescription(Rect thisNodeRect)
		{
			Color descriptionColor = (target as CommentNode).TextColor;
			NodeEditorPreferences.Settings prefs = NodeEditorPreferences.GetSettings();
			GUIStyle descriptionStyle = XNodeUtils.ZoomBasedStyle(30f, 80f, NodeEditorWindow.current.zoom, prefs.minZoom, prefs.maxZoom,  descriptionColor);

			Rect descriptionRect = thisNodeRect;
			descriptionRect.y += 40f;
			descriptionRect.height = Mathf.Min(100f, descriptionRect.height / 3f);
			
			string descriptionText = !(target as CommentNode).Description.IsNullOrWhitespace()
				? (target as CommentNode).Description
				: "<Empty>";
			
			(target as CommentNode).Description = 
				EditorGUI.TextField(descriptionRect, descriptionText, descriptionStyle);
			
			Color descriptionRectColor = (target as CommentNode).Color;
			descriptionRectColor.a = .1f;
			
			EditorGUI.DrawRect(descriptionRect, descriptionRectColor);

			Color borderColor = (target as CommentNode).Color;
			borderColor.a = .5f;
			XNodeUtils.DrawBorderAroundRect(descriptionRect, 10f, borderColor);
		}

		void SelectNodesInside()
		{
			//End the current GUI Area that is restricted to node's dimensions
			GUILayout.EndArea();
			Vector2 nodePos = NodeEditorWindow.current.GridToWindowPositionNoClipped(target.position);
			
			var thisNodeRect = new Rect(nodePos, (target as CommentNode).Dimensions);
			
			//Select nodes within the comment node
			var nodesInGraph = target.graph.nodes;
			foreach (var node in nodesInGraph)
			{
				var nodeWindowPos = 
					NodeEditorWindow.current.GridToWindowPositionNoClipped(node.position);

				if (thisNodeRect.Contains(nodeWindowPos) && !Selection.Contains(node))
				{
					Selection.objects = Selection.objects.Append(node).ToArray();
				}
			}
			
			//Put back the GUI area that is restricted to node's dimensions
			GUILayout.BeginArea(new Rect(nodePos, new Vector2(GetWidth(), 4000)));
		}
}
