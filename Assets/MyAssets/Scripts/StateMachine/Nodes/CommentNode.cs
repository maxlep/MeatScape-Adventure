using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

public class CommentNode : Node
{
    public int Width = 400;
    public int Height = 400;
    
    public Vector2 Dimensions => new Vector2(Width, Height);

    public Color Color = new Color(1f, 1f, 1f, 0.18f);
    public Color TextColor = new Color(1f, 1f, 1f, 1f);

    [Multiline] public string Description = "Description";

    public bool Minimized = false;

    public override object GetValue(NodePort port)
    {
        return null;
    }
}
