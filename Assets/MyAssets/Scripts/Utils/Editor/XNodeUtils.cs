using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using XNodeEditor;

public static class XNodeUtils
{
    //Override that doesnt require color since cant set default value for color
    public static GUIStyle ZoomBasedStyle(float minFontSize, float maxFontSize, FontStyle fontStyle = FontStyle.Bold, 
        TextAnchor anchor = TextAnchor.UpperLeft, bool wordWrap = true, float maxZoom = 5f)
    {
        return ZoomBasedStyle(minFontSize, maxFontSize, Color.white, fontStyle, anchor, wordWrap, maxZoom);
    }
    
    public static GUIStyle ZoomBasedStyle(float minFontSize, float maxFontSize, Color fontColor, FontStyle fontStyle = FontStyle.Bold, 
        TextAnchor anchor = TextAnchor.UpperLeft, bool wordWrap = true, float maxZoom = 5f)
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = Mathf.CeilToInt(Mathf.Lerp(minFontSize, maxFontSize, NodeEditorWindow.current.zoom / maxZoom));
        style.fontStyle = fontStyle;
        style.normal.textColor = fontColor;
        style.alignment = anchor;
        style.wordWrap = wordWrap;

        return style;
    }
    
}
