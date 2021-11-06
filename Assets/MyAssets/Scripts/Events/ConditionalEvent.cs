using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace MyAssets.Scripts.Events
{
    public class ConditionalEvent : SerializedMonoBehaviour
    {
        [SerializeField] [Required] 
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(1f, .85f, .9f)]
        private List<StateMachineGraph> StateMachines;
        
        [Tooltip("Event is fired when ANY of these states are entered")]
        [SerializeField] [Required] 
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)]  [GUIColor(.88f, 1f, .95f)]
        private List<StateNode> ValidStates;

        [Tooltip("Event is fired when NONE of the states above are active and we just exited one or more of them")]
        [SerializeField] [Required]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)]  [GUIColor(.88f, 1f, .95f)]
        private List<StateNode> InvalidStates;
        
        [Tooltip("Transition only valid if ALL of these Bool condition are met")] [ListDrawerSettings(DraggableItems = false)]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)]
        [Required] [HideReferenceObjectPicker]
        [OdinSerialize] private List<BoolCondition> BoolConditions = new List<BoolCondition>();

        [Tooltip("Transition only valid if ALL of these Float condition are met")] [ListDrawerSettings(DraggableItems = false)]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)]
        [Required] [HideReferenceObjectPicker]
        [OdinSerialize] private List<FloatCondition> FloatConditions = new List<FloatCondition>();

        [Tooltip("Transition only valid if ALL of these Int condition are met")] [ListDrawerSettings(DraggableItems = false)]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)]
        [Required] [HideReferenceObjectPicker]
        [OdinSerialize] private List<IntCondition> IntConditions = new List<IntCondition>();
        
        [Tooltip("Transition only valid if ALL of these Int condition are met")] [ListDrawerSettings(DraggableItems = false)]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)]
        [Required] [HideReferenceObjectPicker]
        [SerializeField] private List<TimerCondition> TimerConditions = new List<TimerCondition>();
        
        [Tooltip("Transition only valid if ALL of these Vector2 condition are met")] [ListDrawerSettings(DraggableItems = false)]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)]
        [Required] [HideReferenceObjectPicker]
        [OdinSerialize] private List<Vector2Condition> Vector2Conditions = new List<Vector2Condition>();
        
        [Tooltip("Transition only valid if ALL of these Vector3 condition are met")] [ListDrawerSettings(DraggableItems = false)]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)]
        [Required] [HideReferenceObjectPicker]
        [OdinSerialize] private List<Vector3Condition> Vector3Conditions = new List<Vector3Condition>();
        
        [Tooltip("Transition only valid if ALL of these GameEvent condition are met")] [ListDrawerSettings(DraggableItems = false)]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)]
        [Required] [HideReferenceObjectPicker]
        [OdinSerialize] private List<GameEventCondition> GameEventConditions = new List<GameEventCondition>();
    
        [Tooltip("Transition only valid if ALL of these DynamicGameEvent condition are met")] [ListDrawerSettings(DraggableItems = false)]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)]
        [Required] [HideReferenceObjectPicker]
        [OdinSerialize] private List<DynamicGameEventCondition> DynamicGameEventConditions = new List<DynamicGameEventCondition>();

        public UnityEvent Response;

        private List<ITransitionCondition> allConditions = new List<ITransitionCondition>();

        private bool stopListening;

        private void Awake()
        {
            //Here add all conditions to master list for evaluation and init
            allConditions.Clear();
            allConditions = allConditions.Union(BoolConditions).ToList();
            allConditions = allConditions.Union(FloatConditions).ToList();
            allConditions = allConditions.Union(IntConditions).ToList();
            allConditions = allConditions.Union(TimerConditions).ToList();
            allConditions = allConditions.Union(Vector2Conditions).ToList();
            allConditions = allConditions.Union(Vector3Conditions).ToList();
            allConditions = allConditions.Union(GameEventConditions).ToList();
            allConditions = allConditions.Union(DynamicGameEventConditions).ToList();
            
            //Init all conditions (Really just for the events)
            allConditions.ForEach(c => c.Init(""));
            
            //Subscribe to game events for try raise.
            GameEventConditions.ForEach(e => e.TargetParameter.Subscribe(TryRaise));
            DynamicGameEventConditions.ForEach(e => e.TargetParameter.Subscribe((p, c) => TryRaise()));
        }

        private void Start()
        {
            foreach (var stateMachine in StateMachines)
            {
                stateMachine.onChangeState += EvaluateStateChange;
            }
        }

        private void EvaluateStateChange(StateNode exitingState, StateNode enteringState)
        {
            TryRaise(exitingState, enteringState);
        }

        //For call by events
        public void TryRaise()
        {
            if (stopListening) return;
            TryRaise(null, null);
            GameEventConditions.ForEach(e => e.ResetGameEvent());
            DynamicGameEventConditions.ForEach(e => e.ResetGameEvent());
        }

        //For call by subscription to state change event
        public void TryRaise(StateNode exitingState, StateNode enteringState)
        {
            if (stopListening) return;
            
            #region Valid State Check

            if (!ValidStates.IsNullOrEmpty())
            {
                //Return if no valid states are active
                bool noneValid = true;
                foreach (var stateMachine in StateMachines)
                foreach (var validState in ValidStates)
                    if (stateMachine.currentStates.Contains(validState))
                        noneValid = false;

                if (noneValid) return;
            }
            

            #endregion

            #region Invalid State Check

            if (!InvalidStates.IsNullOrEmpty())
            {
                //Return if any invalid state is active
                foreach (var stateMachine in StateMachines)
                foreach (var invalidState in InvalidStates)
                    if (stateMachine.currentStates.Contains(invalidState))
                        return;

            }

            #endregion

            #region Condition Check

            //Check all Conditions (AND)
            if (!allConditions.IsNullOrEmpty())
            {
                foreach (var condition in allConditions)
                {
                    var result = condition.Evaluate(null);
                    if (!result) return;
                }
            }

            Response.Invoke();

            #endregion
        }

        public void StopListening()
        {
            stopListening = true;
        }
    }
}