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
using XNodeEditor.Internal;
using Node = XNode.Node;

[NodeEditor.CustomNodeEditorAttribute(typeof(CommentNode))]
public class CommentNodeEditor : NodeEditor
{
    bool isDragging = false;
    float lastClickTime = Mathf.NegativeInfinity;
    private Vector2 lastClickPos;
    private float doubleClickTime = 1f;
    
    public static Texture2D corner { get { return _corner != null ? _corner : _corner = Resources.Load<Texture2D>("xnode_corner"); } }
    private static Texture2D _corner;

    public override void OnBodyGUI()
		{
			if (Selection.objects.Length == 1 && Selection.Contains(target))
				NodeEditorWindow.current.Repaint();

			if ((target as CommentNode).Minimized)
				return;

			var thisNodeRect = GUILayoutUtility.GetRect((target as CommentNode).Width, (target as CommentNode).Height);

			//Draw header rect
			DrawNodeHeaderRect(thisNodeRect);
			
			//Corner Drag rect
			var dragRect = DrawDragRect(thisNodeRect);
			
			//Draw node description
			DrawNodeDescription(thisNodeRect);

			if (Selection.objects.Contains(target))
			{
				Event e = Event.current;
				switch (e.type)
				{
					case EventType.MouseDrag:
						
						if (isDragging)
						{
							(target as CommentNode).Width = Mathf.Max(200, (target as CommentNode).Width + (int)e.delta.x);
							(target as CommentNode).Height = Mathf.Max(200, (target as CommentNode).Height + (int)e.delta.y);
						}
						
						break;

					case EventType.MouseDown:
						
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
						
						break;

					case EventType.MouseUp:
						SelectGroup();
						isDragging = false;
						break;
					case EventType.Repaint:
						SendToBack();
						EditorGUIUtility.AddCursorRect(dragRect, MouseCursor.ResizeUpLeft);
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
			//NodeEditorWindow.current.Repaint();
		}

		void DrawNodeHeaderRect(Rect thisNodeRect)
		{
			Color headerColor = (target as CommentNode).TextColor;

			Rect headerRect = thisNodeRect;
			headerRect.y -= 25f;
			headerRect.height = 50f;

			headerColor.a = .1f;
			EditorGUI.DrawRect(headerRect, headerColor);
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
			dragRectColor.a = .05f;
			
			EditorGUI.DrawRect(dragRect, dragRectColor);

			dragRectColor.a = .2f;
			XNodeUtils.DrawBorderAroundRect(dragRect, 5f, dragRectColor);
			GUI.DrawTexture(dragRect, corner);

			return dragRect;
		}

		void DrawNodeDescription(Rect thisNodeRect)
		{
			Color descriptionColor = (target as CommentNode).TextColor;
			NodeEditorPreferences.Settings prefs = NodeEditorPreferences.GetSettings();
			GUIStyle descriptionStyle = XNodeUtils.ZoomBasedStyle(120f, 120f, 
				NodeEditorWindow.current.zoom, prefs.minZoom, prefs.maxZoom,  descriptionColor);

			Rect descriptionRect = thisNodeRect;
			descriptionRect.y += 25f;
			descriptionRect.height = 140f;
			
			string descriptionText = !(target as CommentNode).Description.IsNullOrWhitespace()
				? (target as CommentNode).Description
				: "<Empty>";
			
			(target as CommentNode).Description = 
				EditorGUI.TextField(descriptionRect, descriptionText, descriptionStyle);
			
			Color descriptionRectColor = (target as CommentNode).Color;
			descriptionRectColor.a = .03f;
			
			EditorGUI.DrawRect(descriptionRect, descriptionRectColor);

			Color borderColor = (target as CommentNode).Color;
			borderColor.a = .3f;
			XNodeUtils.DrawBorderAroundRect(descriptionRect, 10f, borderColor);
			EditorGUIUtility.AddCursorRect(descriptionRect, MouseCursor.Text);
			
		}

		void SelectGroup()
		{
			if (Selection.Contains(target)) {
				var selection = Selection.objects.ToList();
				// Select Nodes
				selection.AddRange((target as CommentNode).GetNodesInside());
				// Select Reroutes
				foreach (Node node in target.graph.nodes) {
					foreach (NodePort port in node.Ports) {
						for (int i = 0; i < port.ConnectionCount; i++) {
							List<Vector2> reroutes = port.GetReroutePoints(i);
							for (int k = 0; k < reroutes.Count; k++) {
								Vector2 p = reroutes[k];
								if (p.x < target.position.x) continue;
								if (p.y < target.position.y) continue;
								if (p.x > target.position.x + (target as CommentNode).Width) continue;
								if (p.y > target.position.y + (target as CommentNode).Height + 30) continue;
								if (NodeEditorWindow.current.selectedReroutes.Any(x => x.port == port && x.connectionIndex == i && x.pointIndex == k)) continue;
								NodeEditorWindow.current.selectedReroutes.Add(
									new RerouteReference(port, i, k)
								);
							}
						}
					}
				}
				Selection.objects = selection.Distinct().ToArray();
			}
		}

		void SelectNodesInside()
		{
			//End the current GUI Area that is restricted to node's dimensions
			GUILayout.EndArea();
			Vector2 nodePos = NodeEditorWindow.current.GridToWindowPositionNoClipped(target.position);
			
			var thisNodeRect = new Rect(nodePos, (target as CommentNode).Dimensions);

			var nodesInside = (target as CommentNode).GetNodesInside();
			Selection.objects = Selection.objects.Union(nodesInside).ToArray();
			
			//Put back the GUI area that is restricted to node's dimensions
			GUILayout.BeginArea(new Rect(nodePos, new Vector2(GetWidth(), 4000)));
		}
}
