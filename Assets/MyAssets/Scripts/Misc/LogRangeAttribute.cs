using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Add this attribute to a float property to make it a logarithmic range slider
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class LogRangeAttribute : PropertyAttribute
{
    public float min;
    public float center;
    public float max;

    /// <summary>
    /// Creates a float property slider with a logarithmic 
    /// </summary>
    /// <param name="min">Minimum range value</param>
    /// <param name="center">Value at the center of the range slider</param>
    /// <param name="max">Maximum range value</param>
    public LogRangeAttribute(float min, float center, float max)
    {
        this.min = min;
        this.center = center;
        this.max = max;
    }
}
    
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(LogRangeAttribute))]
public class LogRangePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        LogRangeAttribute logRangeAttribute = (LogRangeAttribute)attribute;
        LogRangeConverter rangeConverter = new LogRangeConverter(logRangeAttribute.min, logRangeAttribute.center, logRangeAttribute.max);

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.PrefixLabel (position, GUIUtility.GetControlID (FocusType.Passive), label);

        float value = rangeConverter.ToNormalized(property.floatValue);
        value = GUI.HorizontalSlider(position, value, 0, 1);
           
        property.floatValue = rangeConverter.ToRange(value);
        EditorGUI.EndProperty();
    }
}
#endif


public struct LogRangeConverter
{
    public readonly float minValue;
    public readonly float maxValue;
        
    private readonly float a;
    private readonly float b;
    private readonly float c;

    /// <summary>
    /// Set up a scaler
    /// </summary>
    /// <param name="minValue">Value for t = 0</param>
    /// <param name="centerValue">Value for t = 0.5</param>
    /// <param name="maxValue">Value for t = 1.0</param>
    public LogRangeConverter(float minValue, float centerValue, float maxValue)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
            
        a = (minValue * maxValue - (centerValue * centerValue)) / (minValue - 2 * centerValue + maxValue);
        b = ((centerValue - minValue) * (centerValue - minValue)) / (minValue - 2 * centerValue + maxValue);
        c = 2 * Mathf.Log(((maxValue - centerValue) / (centerValue - minValue)));
    }

    /// <summary>
    /// Convers the value in range 0 - 1 to the value in range of minValue - maxValue
    /// </summary>
    public float ToRange(float value01)
    {
        return a + b * Mathf.Exp((c * value01));
    }
        
    /// <summary>
    /// Converts the value in range min-max to a value between 0 and 1 that can be used for a slider
    /// </summary>
    public float ToNormalized(float rangeValue)
    {
        return Mathf.Log((rangeValue - a) / b) / c;
    }
}
