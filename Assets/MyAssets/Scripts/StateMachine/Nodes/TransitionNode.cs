using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;
using XNode;

public class TransitionNode : CollapsableNode
{
    [Input(typeConstraint = TypeConstraint.Strict)] [PropertyOrder(-3)] public StateMachineConnection startingState;
    [Output (connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] [PropertyOrder(-2)] public StateMachineConnection nextState;

    [LabelWidth(100)] [MinValue(0)] [HorizontalGroup("Left", MarginRight = 650f)]
    [SerializeField] private int transitionPriority;
    
    [TextArea(3,10)] [HideLabel] [HideInInspector]
    [SerializeField] private string conditionPreview;
    
    [TextArea(2,2)] [HideLabel] [ShowIf("$collapsed")]
    [SerializeField] private string Description;
    
    [Tooltip("Unity Events that are raised when this transition is applied")] 
    [ListDrawerSettings(Expanded = true, DraggableItems = false)] [Required]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.85f, .85f, .95f)]  [HideIf("$collapsed")]
    [SerializeField] private List<UnityEvent> RaiseOnTransitionUnityEvents = new List<UnityEvent>();
    
    [Tooltip("Game Events that are raised when this transition is applied")] 
    [ListDrawerSettings(Expanded = true, DraggableItems = false)] [Required]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.85f, .85f, .95f)]  [HideIf("$collapsed")]
    [SerializeField] private List<GameEvent> RaiseOnTransitionEvents = new List<GameEvent>();

    [Tooltip("Transition only valid if ANY 1 or more of these states are active in respective state machine")] 
    [ListDrawerSettings(Expanded = true, DraggableItems = false)] [Required]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.88f, 1f, .95f)]  [HideIf("$collapsed")]
    [SerializeField] private List<StateNode> ValidStartStates = new List<StateNode>();
    
    [Tooltip("Transition only valid if NONE of these states are active in respective state machine")] 
    [ListDrawerSettings(Expanded = true, DraggableItems = false)] [Required]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.88f, 1f, .95f)]  [HideIf("$collapsed")]
    [SerializeField] private List<StateNode> InvalidStartStates = new List<StateNode>();

    [Tooltip("Transition only valid if ALL of these Bool condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$collapsed")]
    [OnValueChanged("InitConditions")] [Required] [HideReferenceObjectPicker]
    [OdinSerialize] private List<BoolCondition> BoolConditions = new List<BoolCondition>();

    [Tooltip("Transition only valid if ALL of these Trigger condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$collapsed")]
    [OnValueChanged("InitConditions")] [Required]
    [SerializeField] private List<TriggerCondition> TriggerConditions = new List<TriggerCondition>();

    [Tooltip("Transition only valid if ALL of these Float condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$collapsed")]
    [OnValueChanged("InitConditions")] [Required] [HideReferenceObjectPicker]
    [OdinSerialize] private List<FloatCondition> FloatConditions = new List<FloatCondition>();

    [Tooltip("Transition only valid if ALL of these Int condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$collapsed")]
    [OnValueChanged("InitConditions")] [Required] [HideReferenceObjectPicker]
    [OdinSerialize] private List<IntCondition> IntConditions = new List<IntCondition>();
    
    [Tooltip("Transition only valid if ALL of these Int condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$collapsed")]
    [OnValueChanged("InitConditions")] [Required]
    [SerializeField] private List<TimerCondition> TimerConditions = new List<TimerCondition>();
    
    [Tooltip("Transition only valid if ALL of these Vector2 condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$collapsed")]
    [OnValueChanged("InitConditions")] [Required] [HideReferenceObjectPicker]
    [OdinSerialize] private List<Vector2Condition> Vector2Conditions = new List<Vector2Condition>();
    
    [Tooltip("Transition only valid if ALL of these Vector3 condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$collapsed")]
    [OnValueChanged("InitConditions")] [Required] [HideReferenceObjectPicker]
    [OdinSerialize] private List<Vector3Condition> Vector3Conditions = new List<Vector3Condition>();
    
    [Tooltip("Transition only valid if ALL of these GameEvent condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$collapsed")]
    [OnValueChanged("InitConditions")] [Required] [HideReferenceObjectPicker]
    [OdinSerialize] private List<GameEventCondition> GameEventConditions = new List<GameEventCondition>();
    
    [Tooltip("Transition only valid if ALL of these DynamicGameEvent condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$collapsed")]
    [OnValueChanged("InitConditions")] [Required] [HideReferenceObjectPicker]
    [OdinSerialize] private List<DynamicGameEventCondition> DynamicGameEventConditions = new List<DynamicGameEventCondition>();
    
    [Tooltip("Transition only valid if ALL of these DynamicGameEvent condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$collapsed")]
    [OnValueChanged("InitConditions")] [Required] [HideReferenceObjectPicker]
    [OdinSerialize] private List<TransformCondition> TransformConditions = new List<TransformCondition>();
    
    private List<TriggerVariable> triggerVars = new List<TriggerVariable>();
    private List<ITransitionCondition> allConditions = new List<ITransitionCondition>();

    public List<TriggerVariable> TriggerVars => triggerVars;
    public int TransitionPriority => transitionPriority;
    public string ConditionPreview => conditionPreview;

    protected bool isInitialized = false;
    private List<VariableContainer> parameterList;
    private StateMachineGraph stateMachineGraph;
    private string startingStateName;
    private string nextStateName;

    public bool IsInitialized
    {
        get => isInitialized;
        set => isInitialized = value;
    }

    public virtual void Initialize(StateMachineGraph parentGraph)
    {
        this.stateMachineGraph = parentGraph;
        triggerVars.Clear();
        isInitialized = true;
        InitNodeName();

        //Here add all conditions to master list for evaluation and init
        allConditions.Clear();
        allConditions = allConditions.Union(BoolConditions).ToList();
        allConditions = allConditions.Union(TriggerConditions).ToList();
        allConditions = allConditions.Union(FloatConditions).ToList();
        allConditions = allConditions.Union(IntConditions).ToList();
        allConditions = allConditions.Union(TimerConditions).ToList();
        allConditions = allConditions.Union(Vector2Conditions).ToList();
        allConditions = allConditions.Union(Vector3Conditions).ToList();
        allConditions = allConditions.Union(GameEventConditions).ToList();
        allConditions = allConditions.Union(DynamicGameEventConditions).ToList();
        allConditions = allConditions.Union(TransformConditions).ToList();
        InitConditions();
        
        PopulateTriggerList();
    }
    
    public virtual void RuntimeInitialize()
    {
        
    }

    private void InitNodeName()
    {
        var inputConnection = GetInputPort("startingState").Connection;
        StateNode outputState = GetNextState();

        if (inputConnection == null || outputState == null)
            return;
        
        StateNode inputState = inputConnection.node as StateNode;
        StateReferenceNode inputReferenceState = inputConnection.node as StateReferenceNode;
        AnyStateNode inputAnyState = inputConnection.node as AnyStateNode;
        
        if (inputState != null)
            startingStateName = inputState.GetName();
        else if (inputReferenceState != null && inputReferenceState.ReferencedState != null)
            startingStateName = inputReferenceState.ReferencedState.GetName();
        else if (inputAnyState != null)
            startingStateName = "Any State";
        else
            startingStateName = "<Missing>";
        
        nextStateName = outputState != null ? outputState.GetName() : "<Missing>";
        name = $"{startingStateName} -> {nextStateName}";
    }

    private void PopulateTriggerList()
    {
        foreach (var triggerCondition in TriggerConditions)
        {
            if (triggerCondition.TargetParameter != null) 
                triggerVars.Add(triggerCondition.TargetParameter);
        }
    }

    public StateNode GetNextState()
    {
        var outputConnection = GetOutputPort("nextState").Connection;

        if (outputConnection == null) return null;
        
        StateNode connectionAsState = outputConnection.node as StateNode;
        StateReferenceNode connectionAsRef = outputConnection.node as StateReferenceNode;

        if (connectionAsState != null)
            return connectionAsState;

        else if (connectionAsRef != null)
            return connectionAsRef.ReferencedState;

        return null;
    }
    
    public StateNode GetStartingState()
    {
        var inputConnection = GetInputPort("startingState").Connection;

        if (inputConnection == null) return null;
        
        StateNode connectionAsState = inputConnection.node as StateNode;
        StateReferenceNode connectionAsRef = inputConnection.node as StateReferenceNode;

        if (connectionAsState != null)
            return connectionAsState;

        else if (connectionAsRef != null)
            return connectionAsRef.ReferencedState;

        return null;
    }

    private void OnValidate()
    {
        InitConditions();
        name = $"{startingStateName} -> {nextStateName}";
    }

    public void StartTimers()
    {
        foreach (var timerCondition in TimerConditions)
        {
            timerCondition.StartTimer();
        }
    }

    private void InitConditions()
    {
        conditionPreview = "";
        
        foreach (var validState in ValidStartStates)
        {
            conditionPreview += $"- Valid: {validState.GetName()}\n";
        }
        
        foreach (var inValidState in InvalidStartStates)
        {
            conditionPreview += $"- Invalid: {inValidState.GetName()}\n";
        }

        foreach (var condition in allConditions)
        {
            condition.Init(name);
            conditionPreview += $"- {condition}\n";
        }
        
        conditionPreview = string.Concat(conditionPreview.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

    }


    //Return true if all conditions are met
    //Optional trigger can be sent
    public bool EvaluateConditions(List<TriggerVariable> receivedTriggers = null)
    {
        bool result = true;

        //Check Valid start states (OR)
        if (!ValidStartStates.IsNullOrEmpty())
        {
            bool isValidStateActive = false;
            List<StateNode> activeStates = stateMachineGraph.GetActiveStates();

            ValidStartStates.ForEach(validState => activeStates.ForEach(activeState =>
            {
                if (validState == activeState) isValidStateActive = true;
            }));
            if (!isValidStateActive) return false;
        }
        
        //Check inValid start states (OR)
        if (!InvalidStartStates.IsNullOrEmpty())
        {
            bool isInvalidStateAcitve = false;

            isInvalidStateAcitve = stateMachineGraph.CheckInvalidStateActive(InvalidStartStates);
      
            
            if (isInvalidStateAcitve) return false;
        }
        
        //Check Conditions (AND)
        if (!allConditions.IsNullOrEmpty())
        {
            foreach (var condition in allConditions)
            {
                result &= condition.Evaluate(receivedTriggers);
                if (!result) return false;
            }
        }

        return result;
    }

    public void ResetTriggers()
    {
        foreach (var triggerCondition in TriggerConditions)
        {
            triggerCondition.ResetTriggers();
        }
    }

    public void ResetGameEvents()
    {
        GameEventConditions.ForEach(e => e.ResetGameEvent());
        DynamicGameEventConditions.ForEach(e => e.ResetGameEvent());
    }

    public void RaiseTransitionEvents()
    {
        //Raise the unity and game events for valid transition
        RaiseOnTransitionEvents.ForEach(e => e?.Raise());
        RaiseOnTransitionUnityEvents.ForEach(e => e?.Invoke());
    }
    
}