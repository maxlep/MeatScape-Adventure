using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

public class CollapsableNode : Node
{
    [SerializeField] [HideInInspector] protected bool collapsed = true;
    
    public bool Collapsed
    {
        get => collapsed;
        set => collapsed = value;
    }
    
    [HorizontalGroup("split", 30f)] [PropertyOrder(-1)]
    [Button(ButtonSizes.Medium, ButtonStyle.CompactBox, Name = "$GetCollapseButtonName")]
    public void ToggleCollapse()
    {
        collapsed = !collapsed;
    }

    private string GetCollapseButtonName()
    {
        return collapsed ? "+" : "-";
    }
}
