using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

public class CommentNode : Node
{
    public int Width = 800;
    public int Height = 500;
    
    public Vector2 Dimensions => new Vector2(Width, Height);

    public Color Color = new Color(1f, 1f, 1f, 0.08f);
    public Color TextColor = new Color(1f, 1f, 1f, 1f);

    [Multiline] public string Description = "Comment";

    public bool Minimized = false;

    /// <summary> Gets nodes in this group </summary>
    public List<Node> GetNodesInside() {
        List<Node> result = new List<Node>();
        foreach (Node node in graph.nodes) {
            if (node == this) continue;
            if (node.position.x + 250f < this.position.x) continue;
            if (node.position.y < this.position.y) continue;
            if (node.position.x + 250f > this.position.x + Width) continue;
            if (node.position.y > this.position.y + Height + 30) continue;
            result.Add(node);
        }
        return result;
    }

    public override object GetValue(NodePort port)
    {
        return null;
    }

    private void OnValidate()
    {
        name = "";
    }
}
