using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;



[Required]
[CreateAssetMenu(fileName = "VariableContainer", menuName = "VariableContainer", order = 0)]
public class VariableContainer : ScriptableObject
{
    [FolderPath (RequireExistingPath = true), PropertyOrder(-2)]
    [SerializeField] private string FolderPath;
    [SerializeField] private bool IncludeSubdirectories = false;
    [TextArea (5, 10)] public String Description;
    
    [Required] [PropertySpace(SpaceBefore = 10, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
    [SerializeField] private List<TriggerVariable> TriggerVariables = new List<TriggerVariable>();
    
    [Required] [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
    [SerializeField] private List<BoolVariable> BoolVariables = new List<BoolVariable>();
    
    [Required] [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
    [SerializeField] private List<IntVariable> IntVariables = new List<IntVariable>();
    
    [Required] [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
    [SerializeField] private List<FloatVariable> FloatVariables = new List<FloatVariable>();
    
    [Required] [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
    [SerializeField] private List<Vector2Variable> Vector2Variables = new List<Vector2Variable>();
    
    [Required] [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
    [SerializeField] private List<Vector3Variable> Vector3Variables = new List<Vector3Variable>();
    
    [Required] [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
    [SerializeField] private List<QuaternionVariable> QuaternionVariables = new List<QuaternionVariable>();
    
    [Required] [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
    [SerializeField] private List<TimerVariable> TimerVariables = new List<TimerVariable>();

    [Required] [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [ListDrawerSettings(ShowPaging = false)]
    [SerializeField] private List<FunctionVariable> FunctionVariables = new List<FunctionVariable>();
    
    public List<TriggerVariable> GetTriggerVariables() => TriggerVariables;
    public List<BoolVariable> GetBoolVariables() => BoolVariables;
    public List<IntVariable> GetIntVariables() => IntVariables;
    public List<FloatVariable> GetFloatVariables() => FloatVariables;
    public List<Vector2Variable> GetVector2Variables() => Vector2Variables;
    public List<Vector3Variable> GetVector3Variables() => Vector3Variables;
    public List<QuaternionVariable> GetQuaternionVariables() => QuaternionVariables;
    public List<TimerVariable> GetTimerVariables() => TimerVariables;
    public List<FunctionVariable> GetFunctionVariables() => FunctionVariables;

#if UNITY_EDITOR

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
        QuaternionVariables.Clear();
        TimerVariables.Clear();

        foreach (var propertyPath in AssetDatabaseUtils.GetAssetRelativePaths(FolderPath, IncludeSubdirectories))
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
                QuaternionVariables.Add(assetAsQuaternion);
                continue;
            }
            
            TimerVariable assetAsTimer = 
                AssetDatabase.LoadAssetAtPath(propertyPath, typeof(TimerVariable)) as TimerVariable;
            if (assetAsTimer != null)
            {
                TimerVariables.Add(assetAsTimer);
                continue;
            }
            
            FunctionVariable assetAsFunction = 
                AssetDatabase.LoadAssetAtPath(propertyPath, typeof(TimerVariable)) as FunctionVariable;
            if (assetAsFunction != null)
            {
                FunctionVariables.Add(assetAsFunction);
                continue;
            }
        }
        Debug.Log($"{TriggerVariables.Count} Triggers" +
        $" | {BoolVariables.Count} Bools" +
        $" | {IntVariables.Count} Ints" +
        $" | {FloatVariables.Count} Floats" +
        $" | {Vector2Variables.Count} Vector2s" +
        $" | {Vector3Variables.Count} Vector3s" +
        $" | {QuaternionVariables.Count} Quaternions" +
        $" | {TimerVariables.Count} Timers" +
        $" | {FunctionVariables.Count} Functions");
    }
    

    [Button(ButtonSizes.Small), PropertyOrder(-1)]
    public void SetThisFolder()
    {
        string fullPath = AssetDatabase.GetAssetPath(this);
        FolderPath = fullPath.Substring(0, fullPath.LastIndexOf('/'));
    } 
    
#endif
    
}

