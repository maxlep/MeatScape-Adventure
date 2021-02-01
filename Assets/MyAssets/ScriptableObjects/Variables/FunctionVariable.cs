using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    // [Serializable]
    public class ValueOperatorPair
    {
        [HideLabel]
        public FunctionVariableOperators op;
        
        [HideLabel]
        public FloatValueReference value;
        
        internal static float Operate(float previousValue, ValueOperatorPair op)
        {
            switch (op.op)
            {
                case FunctionVariableOperators.Add:
                    return previousValue + op.value.Value;
                case FunctionVariableOperators.Subtract:
                    return previousValue - op.value.Value;
                case FunctionVariableOperators.Multiply:
                    return previousValue * op.value.Value;
                case FunctionVariableOperators.Divide:
                    return previousValue / op.value.Value;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public enum FunctionVariableOperators {
        Add,
        Subtract,
        Multiply,
        Divide,
    }

    internal static class FunctionVariableOperatorsExtensions
    {
        internal static Dictionary<FunctionVariableOperators, string> OperatorToString = new Dictionary<FunctionVariableOperators, string>
        {
            {FunctionVariableOperators.Add, "+"},
            {FunctionVariableOperators.Subtract, "-"},
            {FunctionVariableOperators.Multiply, "*"},
            {FunctionVariableOperators.Divide, "/"},
        };
    }
    
    [Required]
    [CreateAssetMenu(fileName = "FunctionVariable", menuName = "Variables/FunctionVariable", order = 0)]
    public class FunctionVariable : SerializedScriptableObject, IFloatValue
    {
#region Inspector
        
        [TextArea (7, 10)]
        [HideInInlineEditors]
        public string Description;
        
        [Title("$Equation")]
        [LabelText("Operations")]
        [HideReferenceObjectPicker]
        [SerializeField]
        private List<ValueOperatorPair> Operations;
        
        [ShowInInspector]
        [ReadOnly]
        private float _result;
        
#endregion

#region Interface

        public float Value => _result;

        public string GetName() => name;
        public string GetDescription() => Description;
        public float GetValue(Type type) => Value;
        public float GetFloat() => Value;

        public void Subscribe(OnUpdate callback)
        {
            this.OnUpdate += callback;
        }

        public void Unsubscribe(OnUpdate callback)
        {
            this.OnUpdate -= callback;
        }

#endregion
    
        protected event OnUpdate OnUpdate;

        private void OnEnable()
        {
            Recalculate();
            Operations.ForEach((o) => o.value.Subscribe(Recalculate));
        }

        private void Recalculate()
        {
            _result = Operations.Aggregate(0f, ValueOperatorPair.Operate);
            OnUpdate?.Invoke();
        }

        private string Equation
        {
            get
            {
                string equationStr = "Equation: ";
                if (Operations == null || Operations.Count == 0)
                {
                    return equationStr;
                }
                equationStr += "v0";
                for (int i = 1; i < Operations.Count; i++)
                {
                    var op = Operations[i];
                    string opStr = FunctionVariableOperatorsExtensions.OperatorToString[op.op];
                    equationStr += $" {opStr} v{i + 1}";
                }
                return equationStr;
            }
        }
    }
}