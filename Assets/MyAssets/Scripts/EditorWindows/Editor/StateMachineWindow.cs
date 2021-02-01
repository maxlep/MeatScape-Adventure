using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class StateMachineWindow : OdinEditorWindow
{
    [FolderPath (RequireExistingPath = true)] [PropertyOrder(100f)]
    public string FolderPath = "Assets/MyAssets/ScriptableObjects/Graphs/StateMachine";
    
    private const string ASSET_EXTENSION = ".asset";
    
    [MenuItem("Window/State Machine View")]
    private static void OpenWindow()
    {
        GetWindow<StateMachineWindow>().Show();
    }

    [HorizontalGroup] [PropertyOrder(101f)]
    [Button(ButtonSizes.Small)]
    public void Populate()
    {
        StateMachineReferences.Clear();
        
        foreach (var propertyPath in AssetDatabaseUtils.GetAssetRelativePaths(FolderPath, true))
        {
            StateMachineGraph assetAsStateMachine =
                AssetDatabase.LoadAssetAtPath(propertyPath, typeof(StateMachineGraph)) as StateMachineGraph;
            if (assetAsStateMachine != null)
            {
                StateMachineReference stateMachineReference = new StateMachineReference();
                stateMachineReference.StateMachine = assetAsStateMachine;
                StateMachineReferences.Add(stateMachineReference);
                continue;
            }
        }
    }

    [TableList]
    public List<StateMachineReference> StateMachineReferences;

    /*
    [PropertyOrder(-10)]
    [HorizontalGroup]
    [Button(ButtonSizes.Large)]
    public void SomeButton1() { }

    [HorizontalGroup]
    [Button(ButtonSizes.Large)]
    public void SomeButton2() { }

    [HorizontalGroup]
    [Button(ButtonSizes.Large)]
    public void SomeButton3() { }

    [HorizontalGroup]
    [Button(ButtonSizes.Large), GUIColor(0, 1, 0)]
    public void SomeButton4() { }

    [HorizontalGroup]
    [Button(ButtonSizes.Large), GUIColor(1, 0.5f, 0)]
    public void SomeButton5() { }
    */
}

public class StateMachineReference
{
    [AssetsOnly] [HideInInspector]
    public StateMachineGraph StateMachine;

    public string Name => (StateMachine != null) ? StateMachine.name : "Null";

    [Button(ButtonSizes.Small, Name = "$Name"), GUIColor(0, 1, 0)]
    [TableColumnWidth(160)]
    public void OpenGraph()
    {
        AssetDatabase.OpenAsset(StateMachine);
    }
}
