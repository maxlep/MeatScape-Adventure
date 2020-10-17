using System.Collections.Generic;
using MyAssets.ScriptableObjects.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace MyAssets.Scripts.Events
{
    public class ConditionalEvent : MonoBehaviour
    {
        [Tooltip("Transition only valid if ALL of these Float condition are met")] [ListDrawerSettings(Expanded = true, DraggableItems = false)]
        [GUIColor(.9f, .95f, 1f)] [Required] [SerializeField]
        private List<FloatCondition> FloatConditions = new List<FloatCondition>();

        public UnityEvent Response;

        public void TryRaise()
        {
            foreach (var condition in FloatConditions)
            {
                var valid = condition.Evaluate(null);
                //Debug.Log(valid);
                if (!valid) return;
            }

            Response.Invoke();
        }
    }
}