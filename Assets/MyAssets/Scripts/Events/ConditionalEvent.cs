using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Events;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace MyAssets.Scripts.Events
{
    public class ConditionalEvent : MonoBehaviour
    {
        [Tooltip("Transition only valid if ALL of these Bool condition are met")] [ListDrawerSettings(DraggableItems = false)]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)]
        [Required]
        [SerializeField] private List<BoolCondition> BoolConditions = new List<BoolCondition>();

        [Tooltip("Transition only valid if ALL of these Float condition are met")] [ListDrawerSettings(DraggableItems = false)]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)]
        [Required]
        [SerializeField] private List<FloatCondition> FloatConditions = new List<FloatCondition>();

        [Tooltip("Transition only valid if ALL of these Int condition are met")] [ListDrawerSettings(DraggableItems = false)]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)]
        [Required]
        [SerializeField] private List<IntCondition> IntConditions = new List<IntCondition>();
        
        [Tooltip("Transition only valid if ALL of these Int condition are met")] [ListDrawerSettings(DraggableItems = false)]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)]
        [Required]
        [SerializeField] private List<TimerCondition> TimerConditions = new List<TimerCondition>();
        
        [Tooltip("Transition only valid if ALL of these Vector2 condition are met")] [ListDrawerSettings(DraggableItems = false)]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)]
        [Required]
        [SerializeField] private List<Vector2Condition> Vector2Conditions = new List<Vector2Condition>();
        
        [Tooltip("Transition only valid if ALL of these Vector3 condition are met")] [ListDrawerSettings(DraggableItems = false)]
        [PropertySpace(SpaceBefore = 0, SpaceAfter = 10)] [GUIColor(.9f, .95f, 1f)]
        [Required]
        [SerializeField] private List<Vector3Condition> Vector3Conditions = new List<Vector3Condition>();

        public UnityEvent Response;
        
        private List<ITransitionCondition> allConditions = new List<ITransitionCondition>();

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
        }

        public void TryRaise()
        {
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
        }
    }
}