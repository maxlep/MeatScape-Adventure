using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

[Required]
[InlineEditor(InlineEditorObjectFieldModes.Foldout)]
[CreateAssetMenu(fileName = "TimerVariable", menuName = "Variables/TimerVariable", order = 0)]
public class TimerVariable : ScriptableObject
{
    [SerializeField] private float duration;
    [ShowInInspector] private float remaingTime;
    [TextArea (7, 10)] [HideInInlineEditors] public String Description;
    
    public delegate void OnUpdate_();
    public event OnUpdate_ OnUpdate;

    public float Duration => duration;
    public float RemainingTime => remaingTime;
    public float ElapsedTime => duration - remaingTime;

    public bool IsStopped => isStopped;

    private float startTime = Mathf.NegativeInfinity;

    private bool isStopped = true;
    
    public void StartTimer()
    {
        isStopped = false;
        startTime = Time.time;
    }

    public void StopTimer()
    {
        isStopped = true;
    }

    public void UpdateTime()
    {
        if (!isStopped) remaingTime = Mathf.Max(0f, duration - (Time.time - startTime));
    }
    
}

[Serializable]
[InlineProperty]
public class TimerReference
{
    [HorizontalGroup("Split", LabelWidth = .01f)] [PropertyTooltip("$Tooltip")]
    [BoxGroup("Split/Left", ShowLabel = false)] [LabelText("$LabelText")] [LabelWidth(10f)]
    [SerializeField] private bool UseConstant = false;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
    [SerializeField] private float ConstantDuration;
    
    [SerializeField] [ShowIf("UseConstant")] [DisableIf("AlwaysTrue")]
    private float ConstantRemainingTime;

    private bool AlwaysTrue => true;
    private bool isConstantStopped = true;

    public float ElapsedTime
    {
        get
        {
            if (UseConstant)
                return ConstantDuration - ConstantRemainingTime;
            if (Variable != null)
                return Variable.ElapsedTime;
            
            Debug.LogError("Trying to access elapsed time for timer variable that is not set!");
            return Mathf.Infinity;
        }
    }

    public bool IsStopped => (UseConstant) ? isConstantStopped : Variable.IsStopped; 

    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")] 
    [SerializeField] private TimerVariable Variable;

    private float startTime = Mathf.NegativeInfinity;

    public String Tooltip => Variable != null && !UseConstant ? Variable.Description : "";
    public String LabelText => UseConstant ? "" : "?";

    public float Duration
    {
        get
        {
            if (UseConstant)
                return ConstantDuration;
            if (Variable != null)
                return Variable.Duration;
            
            Debug.LogError("Trying to access duration for timer variable that is not set!");
            return Mathf.Infinity;
        }
    }

    public float RemainingTime
    {
        get
        {
            if (UseConstant)
                return ConstantRemainingTime;
            if (Variable != null)
                return Variable.RemainingTime;
            
            Debug.LogError("Trying to access remaining time for timer variable that is not set!");
            return Mathf.Infinity;
        }
    }


    public String Name
    {
        get
        {
            if (UseConstant) 
                return $"<Const>{ConstantRemainingTime}";
                
            return (Variable != null) ? Variable.name : "<Missing Timer>";
        }
    }


    public void StartTimer()
    {
        Variable?.StartTimer();
        isConstantStopped = false;
        startTime = Time.time;
    }

    public void StopTimer()
    {
        Variable?.StopTimer();
        isConstantStopped = true;
    }

    public void UpdateTime()
    {
        Variable?.UpdateTime();
        if (!isConstantStopped) ConstantRemainingTime = Mathf.Max(0f, ConstantDuration - (Time.time - startTime));
    }
    
}