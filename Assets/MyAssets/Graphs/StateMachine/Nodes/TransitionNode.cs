using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using XNode;

public class TransitionNode : Node
{
    [Input(typeConstraint = TypeConstraint.Strict)] [PropertyOrder(-3)] public StateMachineConnection startingState;
    [Output (connectionType = ConnectionType.Override, typeConstraint = TypeConstraint.Strict)] [PropertyOrder(-2)] public StateMachineConnection nextState;

    [TextArea(3,10)] [HideLabel]
    [SerializeField] private string conditionPreview;

    [Tooltip("Transition only valid if ANY 1 or more of these states are active in OTHER state machine")] 
    [ListDrawerSettings(Expanded = true, DraggableItems = false)] [Required]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.88f, 1f, .95f)]  [HideIf("$zoom")]
    [SerializeField] private List<StateNode> ValidStartStates = new List<StateNode>();
    
    [Tooltip("Transition only valid if ANY 1 or more of these states are not active in OTHER state machine")] 
    [ListDrawerSettings(Expanded = true, DraggableItems = false)] [Required]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.88f, 1f, .95f)]  [HideIf("$zoom")]
    [SerializeField] private List<StateNode> InvalidStartStates = new List<StateNode>();

    [Tooltip("Transition only valid if ALL of these Bool condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$zoom")]
    [OnValueChanged("InitConditions")] [Required]
    [SerializeField] private List<BoolCondition> BoolConditions = new List<BoolCondition>();

    [Tooltip("Transition only valid if ALL of these Trigger condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$zoom")]
    [OnValueChanged("InitConditions")] [Required]
    [SerializeField] private List<TriggerCondition> TriggerConditions = new List<TriggerCondition>();

    [Tooltip("Transition only valid if ALL of these Float condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$zoom")]
    [OnValueChanged("InitConditions")] [Required]
    [SerializeField] private List<FloatCondition> FloatConditions = new List<FloatCondition>();

    [Tooltip("Transition only valid if ALL of these Int condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$zoom")]
    [OnValueChanged("InitConditions")] [Required]
    [SerializeField] private List<IntCondition> IntConditions = new List<IntCondition>();
    
    [Tooltip("Transition only valid if ALL of these Int condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$zoom")]
    [OnValueChanged("InitConditions")] [Required]
    [SerializeField] private List<TimerCondition> TimerConditions = new List<TimerCondition>();
    
    [Tooltip("Transition only valid if ALL of these Vector2 condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$zoom")]
    [OnValueChanged("InitConditions")] [Required]
    [SerializeField] private List<Vector2Condition> Vector2Conditions = new List<Vector2Condition>();
    
    [Tooltip("Transition only valid if ALL of these Vector3 condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
    [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)] [HideIf("$zoom")]
    [OnValueChanged("InitConditions")] [Required]
    [SerializeField] private List<Vector3Condition> Vector3Conditions = new List<Vector3Condition>();


    private bool zoom = false;
    private List<TriggerVariable> triggerVars = new List<TriggerVariable>();
    private List<ITransitionCondition> allConditions = new List<ITransitionCondition>();

    public List<TriggerVariable> TriggerVars => triggerVars;
    public string ConditionPreview => conditionPreview;


    protected bool isInitialized = false;
    private List<VariableContainer> parameterList;
    private StateMachineGraph stateMachineGraph;
    private string startingStateName;
    private string nextStateName;

    public bool Zoom
    {
        get => zoom;
        set => zoom = value;
    }
    
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
        
        startingStateName = inputState != null ? inputState.GetName() : "Any State";
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
    }


    //Return true if all conditions are met
    //Optional trigger can be sent
    public bool EvaluateConditions(TriggerVariable receivedTrigger = null)
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
                result &= condition.Evaluate(receivedTrigger);
                if (!result) return false;
            }
        }

        return result;
    }
    

    [HorizontalGroup("split", 30f)] [PropertyOrder(-1)]
    [Button(ButtonSizes.Medium, ButtonStyle.CompactBox, Name = "$GetZoomButtonName")]
    public void ToggleZoom()
    {
        zoom = !zoom;
    }

    private string GetZoomButtonName()
    {
        return zoom ? "+" : "-";
    }
}