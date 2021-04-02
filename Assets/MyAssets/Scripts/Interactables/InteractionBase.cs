using System.Collections.Generic;
using System.Linq;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

[HideReferenceObjectPicker]
public class InteractionBase : IInteraction
{
    [SerializeField] private List<IConditional> Conditionals;
    
    protected bool EvaluateConditionals()
    {
        if (Conditionals.IsNullOrEmpty()) return true;

        bool result = Conditionals.Aggregate(true, (accumulator, conditional) =>
        {
            var operation = (conditional as Conditional).Operation.AsFunction();
            return operation(accumulator, conditional.Evaluate());
        });

        return result;
    }

}

internal interface IConditional
{
    public bool Evaluate();
}

[HideReferenceObjectPicker]
internal class Conditional : IConditional
{
    [HideLabel, GUIColor(.85f, .90f, 1f)]
    public LogicalOperator Operation;
    
    [SerializeField]
    [Title("$Equation")]
    private FloatValueReference v1;
    
    [SerializeField, HideLabel]
    private ConditionalOperator conditionalOperator;
    
    [SerializeField]
    private FloatValueReference v2;
        
    public bool Evaluate()
    {
        var operation = conditionalOperator.AsFunction();
        return operation(v1.Value, v2.Value);
    }
    
    private string Equation
    {
        get
        {
            string equationStr = "Equation: ";
            equationStr += "v1";
            equationStr += $" {conditionalOperator.AsString()} ";
            equationStr += "v2";
            return equationStr;
        }
    }
}

internal enum ConditionalOperator
{
    LessThan,
    LessThanEqualTo,
    GreaterThan,
    GreatherThanEqualTo,
    EqualTo
}

internal static class ConditionalOperatorExtensions
{
    public delegate bool Operator(float value1, float value2);
        
    public static Operator AsFunction(this ConditionalOperator op) => OperatorToFunction[op];
    public static string AsString(this ConditionalOperator op) => OperatorToString[op];

    private static bool LessThan(float a, float b) => a < b;
    private static bool LessThanEqualTo(float a, float b) => a <= b;
    private static bool GreaterThan(float a, float b) => a > b;
    private static bool GreatherThanEqualTo(float a, float b) => a >= b;
    private static bool EqualTo(float a, float b) => Mathf.Approximately(a, b);
        
    private static Dictionary<ConditionalOperator, Operator> OperatorToFunction = new Dictionary<ConditionalOperator,Operator>
    {
        {ConditionalOperator.LessThan, LessThan},
        {ConditionalOperator.LessThanEqualTo, LessThanEqualTo},
        {ConditionalOperator.GreaterThan, GreaterThan},
        {ConditionalOperator.GreatherThanEqualTo, GreatherThanEqualTo},
        {ConditionalOperator.EqualTo, EqualTo},
    };
        
    private static Dictionary<ConditionalOperator, string> OperatorToString = new Dictionary<ConditionalOperator, string>
    {
        {ConditionalOperator.LessThan, "<"},
        {ConditionalOperator.LessThanEqualTo, "<="},
        {ConditionalOperator.GreaterThan, ">"},
        {ConditionalOperator.GreatherThanEqualTo, ">="},
        {ConditionalOperator.EqualTo, "="},
    };
}

internal enum LogicalOperator
{
    And,
    Or
}

internal static class LogicalOperatorExtensions
{
    public delegate bool Operator(bool value1, bool value2);
    
    public static Operator AsFunction(this LogicalOperator op) => OperatorToFunction[op];

    private static bool And(bool a, bool b) => a && b;
    private static bool Or(bool a, bool b) => a || b;

        
    private static Dictionary<LogicalOperator, Operator> OperatorToFunction = new Dictionary<LogicalOperator,Operator>
    {
        {LogicalOperator.And, And},
        {LogicalOperator.Or, Or}
    };
}




