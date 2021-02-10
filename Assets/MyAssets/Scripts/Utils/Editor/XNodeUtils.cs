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
    
    //Make sure rect doesnt extend past other
    public static Rect ClampToRect(Rect rectToClamp, Rect other)
    {
        //If Xpos outside left
        if (rectToClamp.position.x < other.position.x)
            rectToClamp.position = new Vector2(other.position.x, rectToClamp.position.y);
            
        //If Ypos outside above
        if (rectToClamp.position.y < other.position.y)
            rectToClamp.position = new Vector2(rectToClamp.position.x, other.position.y);

        //Offsets from other rect
        float offsetX = rectToClamp.position.x - other.position.x;
        float offsetY = rectToClamp.position.y - other.position.y;

        //If extends past width
        if (rectToClamp.size.x + offsetX > other.size.x)
        {
            rectToClamp.size = new Vector2(other.size.x - offsetX, rectToClamp.size.y);
        }
            
        //If extends past height
        if (rectToClamp.size.y + offsetY > other.size.y)
        {
            rectToClamp.size = new Vector2(rectToClamp.size.x, other.size.y - offsetY);
        }

        return rectToClamp;
    }
}
