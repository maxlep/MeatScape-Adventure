using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;
using XNodeEditor;

public static class XNodeUtils
{
    //Override that doesnt require color since cant set default value for color
    public static GUIStyle ZoomBasedStyle(float minFontSize, float maxFontSize, float currentZoom, float minZoom, float maxZoom,
        FontStyle fontStyle = FontStyle.Bold, TextAnchor anchor = TextAnchor.UpperLeft, bool wordWrap = true)
    {
        return ZoomBasedStyle(minFontSize, maxFontSize, currentZoom, minZoom, maxZoom, Color.white, fontStyle, anchor, wordWrap);
    }
    
    public static GUIStyle ZoomBasedStyle(float minFontSize, float maxFontSize, float currentZoom, float minZoom, float maxZoom,
        Color fontColor, FontStyle fontStyle = FontStyle.Bold, TextAnchor anchor = TextAnchor.UpperLeft, bool wordWrap = true)
    {
        GUIStyle style = new GUIStyle();
        style.fontSize = Mathf.CeilToInt(Mathf.Lerp(minFontSize, maxFontSize, Mathf.InverseLerp(minZoom, maxZoom, currentZoom)));
        style.fontStyle = fontStyle;
        style.normal.textColor = fontColor;
        style.alignment = anchor;
        style.wordWrap = wordWrap;

        return style;
    }
    
    public static void DrawBorderAroundRect(Rect rect, float thickness, Color color)
    {
        Vector3[] rectCorners = new Vector3[]
        {
            rect.position,
            rect.position + new Vector2(rect.width, 0f), 
            rect.position + new Vector2(rect.width, rect.height), 
            rect.position + new Vector2(0f, rect.height), 
            rect.position
        };
        Color originalColor = Handles.color;
        Handles.color = color;
        Handles.DrawAAPolyLine(thickness, rectCorners);
        Handles.color = originalColor;
    }
}
