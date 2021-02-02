﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }

    public enum FunctionVariableOperators {
        Add,
        Subtract,
        Multiply,
        Divide,
    }

    internal static class FunctionVariableOperatorsExtensions
    {
        public delegate float Operator(float value1, float value2);
        
        public static Operator ToFunction(this FunctionVariableOperators op) => OperatorToFunction[op];
        public static string ToString(this FunctionVariableOperators op) => OperatorToString[op];
        
        private static float Add(float a, float b) => a + b;
        private static float Subtract(float a, float b) => a - b;
        private static float Multiply(float a, float b) => a * b;
        private static float Divide(float a, float b) => a / b;
        
        private static Dictionary<FunctionVariableOperators, Operator> OperatorToFunction = new Dictionary<FunctionVariableOperators,Operator>
        {
            {FunctionVariableOperators.Add, Add},
            {FunctionVariableOperators.Subtract, Subtract},
            {FunctionVariableOperators.Multiply, Multiply},
            {FunctionVariableOperators.Divide, Divide},
        };
        
        private static Dictionary<FunctionVariableOperators, string> OperatorToString = new Dictionary<FunctionVariableOperators, string>
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
            _result = Operations.Aggregate(0f, (accumulator, op) =>
            {
                var operation = op.op.ToFunction();
                return operation(accumulator, op.value.Value);
            });
            OnUpdate?.Invoke();
        }

        private string Equation
        {
            get
            {
                string equationStr = "Equation:";
                if (Operations == null || Operations.Count == 0)
                {
                    return equationStr;
                }
                var equation = new StringBuilder(equationStr);
                equation.Append(" v0");
                for (int i = 1; i < Operations.Count; i++)
                {
                    equation.Append($" {Operations[i].op.ToString()} v{i + 1}");
                }
                return equation.ToString();
            }
        }
    }
}