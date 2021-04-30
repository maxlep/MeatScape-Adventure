using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ScriptableObjectUtils
{
    public static void SaveInstance(this ScriptableObject so)
    {
        if (!AssetDatabase.Contains(so))
        {
            String guid = so.GetHashCode().ToString();
            String folderPath = "Assets/MyAssets/ScriptableObjects/InstancedProperties/";
            String prefix = "{Instance}";
            String path = $"{folderPath}{prefix}{guid}.asset";
            AssetDatabase.CreateAsset(so, path);
        }

        AssetDatabase.SaveAssets();
    }
}
