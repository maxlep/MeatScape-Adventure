using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ScriptableObjectUtils
{
    public static void SaveInstance(this ScriptableObject so, String label = "")
    {
        if (!AssetDatabase.Contains(so))
        {
            String name = string.IsNullOrEmpty(label) ? name = so.GetHashCode().ToString() : label;
            String folderPath = "Assets/MyAssets/ScriptableObjects/InstancedProperties/";
            String prefix = "{I}";
            String path = $"{folderPath}{prefix} {name}.asset";
            AssetDatabase.CreateAsset(so, path);
        }

        AssetDatabase.SaveAssets();
    }
}
