﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

[Required]
[CreateAssetMenu(fileName = "VariableContainer", menuName = "VariableContainer", order = 0)]
public class VariableContainer : ScriptableObject
{
    [FolderPath (RequireExistingPath = true)]
    [SerializeField] private string FolderPath;
    [SerializeField] private bool IncludeSubdirectories = false;
    
    [PropertySpace(SpaceBefore = 10, SpaceAfter = 10)]
    [SerializeField] private List<TriggerVariable> TriggerVariables = new List<TriggerVariable>();
    
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)]
    [SerializeField] private List<BoolVariable> BoolVariables = new List<BoolVariable>();
    
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)]
    [SerializeField] private List<IntVariable> IntVariables = new List<IntVariable>();
    
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)]
    [SerializeField] private List<FloatVariable> FloatVariables = new List<FloatVariable>();
    
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)]
    [SerializeField] private List<Vector2Variable> Vector2Variables = new List<Vector2Variable>();
    
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)]
    [SerializeField] private List<Vector3Variable> Vector3Variables = new List<Vector3Variable>();
    
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)]
    [SerializeField] private List<QuaternionVariable> quaternionVariables = new List<QuaternionVariable>();
    
    private const string ASSET_EXTENSION = ".asset";

    [GUIColor(0, 1, 0)]
    [Button(ButtonSizes.Large)]
    public void PopulateContainer()
    {
        TriggerVariables.Clear();
        BoolVariables.Clear();
        IntVariables.Clear();
        FloatVariables.Clear();
        Vector2Variables.Clear();
        Vector3Variables.Clear();
        quaternionVariables.Clear();

        foreach (var propertyPath in GetAssetRelativePaths(FolderPath))
        {
            TriggerVariable assetAsTrigger = 
                AssetDatabase.LoadAssetAtPath(propertyPath, typeof(TriggerVariable)) as TriggerVariable;
            if (assetAsTrigger != null)
            {
                TriggerVariables.Add(assetAsTrigger);
                continue;
            }
            
            BoolVariable assetAsBool = 
                AssetDatabase.LoadAssetAtPath(propertyPath, typeof(BoolVariable)) as BoolVariable;
            if (assetAsBool != null)
            {
                BoolVariables.Add(assetAsBool);
                continue;
            }
            
            IntVariable assetAsInt = 
                AssetDatabase.LoadAssetAtPath(propertyPath, typeof(IntVariable)) as IntVariable;
            if (assetAsInt != null)
            {
                IntVariables.Add(assetAsInt);
                continue;
            }
            
            FloatVariable assetAsFloat = 
                AssetDatabase.LoadAssetAtPath(propertyPath, typeof(FloatVariable)) as FloatVariable;
            if (assetAsFloat != null)
            {
                FloatVariables.Add(assetAsFloat);
                continue;
            }
            
            Vector2Variable assetAsVector2 = 
                AssetDatabase.LoadAssetAtPath(propertyPath, typeof(Vector2Variable)) as Vector2Variable;
            if (assetAsVector2 != null)
            {
                Vector2Variables.Add(assetAsVector2);
                continue;
            }
            
            Vector3Variable assetAsVector3 = 
                AssetDatabase.LoadAssetAtPath(propertyPath, typeof(Vector3Variable)) as Vector3Variable;
            if (assetAsVector3 != null)
            {
                Vector3Variables.Add(assetAsVector3);
                continue;
            }
            
            QuaternionVariable assetAsQuaternion = 
                AssetDatabase.LoadAssetAtPath(propertyPath, typeof(QuaternionVariable)) as QuaternionVariable;
            if (assetAsQuaternion != null)
            {
                quaternionVariables.Add(assetAsQuaternion);
                continue;
            }
        }
        Debug.Log($"{TriggerVariables.Count} Triggers" +
        $" | {BoolVariables.Count} Bools" +
        $" | {IntVariables.Count} Ints" +
        $" | {FloatVariables.Count} Floats" +
        $" | {Vector2Variables.Count} Vector2s" +
        $" | {Vector3Variables.Count} Vector3s" +
        $" | {quaternionVariables.Count} Quaternions");
    }

    private List<string> GetAssetRelativePaths(string path)
    {
        List<string> assetRelativePaths = new List<string>();
        var info = new DirectoryInfo(path);
        var fileInfo = info.GetFiles();
        foreach (var file in fileInfo)
        {
            if (file.Extension.Equals(ASSET_EXTENSION))
            {
                assetRelativePaths.Add($"{path}/{file.Name}");
            }
        }

        if (IncludeSubdirectories)
        {
            string[] subdirectoryNames = AssetDatabase.GetSubFolders(path);
            subdirectoryNames.ForEach(d => Debug.Log(d)); 
            foreach (var subDir in subdirectoryNames)
            {
                assetRelativePaths = assetRelativePaths.Union(GetAssetRelativePaths(subDir).ToList()).ToList();
            }
        }

        return assetRelativePaths;
    }
}
