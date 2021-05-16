using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class ScriptableObjectUtils
{
    public static bool SaveInstance(this ScriptableObject so, string folderPath, string name = "")
    {
        #if !UNITY_EDITOR
        return false;
        #else
        if (AssetDatabase.Contains(so)) return false;
        
        String label = string.IsNullOrEmpty(name) ? name = so.GetHashCode().ToString() : name;
        String path = $"{folderPath}{label}.asset";
        Debug.Log($"Trying to save ScriptableObject to: {path}");

        if (AssetDatabase.LoadAssetAtPath(path, typeof(Object)) != null)
        {
            Debug.LogError("Failed to save ScriptableObject! Asset with same path/name already exists!");
            return false;
        }
        
        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();
        return true;
        #endif
    }
}
