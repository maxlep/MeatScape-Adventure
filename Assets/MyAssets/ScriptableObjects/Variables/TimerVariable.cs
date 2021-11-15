using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[Required]
[InlineEditor(InlineEditorObjectFieldModes.Foldout)]
[CreateAssetMenu(fileName = "TimerVariable", menuName = "Variables/TimerVariable", order = 0)]
public class TimerVariable : ScriptableObject
{
    [SerializeField] [FormerlySerializedAs("duration")] public float Duration;
    [ShowInInspector, NonSerialized] private float? remainingTime = null;
    [ShowInInspector, NonSerialized] private bool isFinished = false;
    [TextArea (7, 10)] [HideInInlineEditors] public String Description;
    
    public delegate void OnUpdate_();
    public event OnUpdate_ OnUpdate;

    // public float Duration => duration;
    public float? RemainingTime => remainingTime;
    public float ElapsedTime => Duration - (remainingTime ?? Duration);
    public bool IsFinished => isFinished;

    public bool IsStopped => isStopped;

    [NonSerialized]
    private float startTime = Mathf.NegativeInfinity;

    [NonSerialized]
    private bool isStopped = true;
    
    public void StartTimer()
    {
        isStopped = false;
        isFinished = false;
        startTime = Time.time;
    }

    public void ResetTimer()
    {
        isStopped = true;
        isFinished = false;
        startTime = Time.time;
    }

    public void StopTimer()
    {
        isStopped = true;
    }

    public void Subscribe(OnUpdate_ callback)
    {
        this.OnUpdate += callback;
    }

    public void Unsubscribe(OnUpdate_ callback)
    {
        this.OnUpdate -= callback;
    }

    public void UpdateTime()
    {
        if (!isStopped && !isFinished)
        {
            remainingTime = Mathf.Max(0f, Duration - (Time.time - startTime));
            OnUpdate?.Invoke();
            if (remainingTime == 0)
            {
                isFinished = true;
            }
        }
        
    }
    
}

[Serializable]
[InlineProperty]
[SynchronizedHeader]
public class TimerReference
{
    [HorizontalGroup("Split", LabelWidth = .01f, Width = .2f)] [PropertyTooltip("$Tooltip")]
    [BoxGroup("Split/Left", ShowLabel = false)] [LabelText("$LabelText")] [LabelWidth(10f)]
    [SerializeField] private bool UseConstant = false;
    
    [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
    [SerializeField] private float ConstantDuration;
    
    [SerializeField] [ShowIf("UseConstant")] [DisableIf("AlwaysTrue")]
    private float? ConstantRemainingTime = null;

    private bool AlwaysTrue => true;
    private bool isConstantStopped = true;
    private bool isConstantFinished = false;

    public float ElapsedTime
    {
        get
        {
            if (UseConstant)
                return ConstantDuration - (ConstantRemainingTime ?? ConstantDuration);
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
        set
        {
            if (UseConstant)
            {
                ConstantDuration = value;
                return;
            }
            if (Variable != null)
            {
                Variable.Duration = value;
                return;
            }
            
            Debug.LogError("Trying to access duration for timer variable that is not set!");
        }
    }

    public float? RemainingTime
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
    
    public bool IsFinished
    {
        get
        {
            if (UseConstant)
                return isConstantFinished;
            if (Variable != null)
                return Variable.IsFinished;
            
            Debug.LogError("Trying to access IsFinished for timer variable that is not set!");
            return false;
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


    public void RestartTimer()
    {
        Variable?.StartTimer();
        isConstantStopped = false;
        isConstantFinished = false;
        startTime = Time.time;
    }

    public void ResetTimer()
    {
        Variable?.ResetTimer();
        isConstantStopped = true;
        isConstantFinished = false;
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
        if (!isConstantStopped && !isConstantFinished)
        {
            ConstantRemainingTime = Mathf.Max(0f, ConstantDuration - (Time.time - startTime));
            if (ConstantRemainingTime == 0)
            {
                isConstantFinished = true;
            }
        }
    }
    
}