using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using System;
using MyAssets.ScriptableObjects.Variables.ValueReferences;

[Required]
[CreateAssetMenu(fileName = "FunctionVariable", menuName = "Variables/FunctionVariable", order = 0)]
public class FunctionVariable : ScriptableObject, IFloatValue
{
    #region Inspector
    
    [Title("$Equation")]
    [LabelText("Operations")]
    [HideReferenceObjectPicker]
    [SerializeField]
    private List<VariableOperatorPair> VarOpPairs;

    [TextArea (7, 10)]
    [HideInInlineEditors]
    public string Description;
    
    #endregion

    #region Interface

    public float Value { get; private set; }

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

    void Awake() {
        this.Recalculate();
        VarOpPairs.ForEach((varOp) => {
            varOp.variable.Subscribe(this.Recalculate);
        });
    }

    private void Recalculate() {
        float value = VarOpPairs[0].variable.Value;
        FunctionVariableOperators op = VarOpPairs[0].op;
        foreach(var varOpPair in VarOpPairs.GetRange(1, VarOpPairs.Count - 1)) {
            if(op == FunctionVariableOperators.Add) value += varOpPair.variable.Value;
            if(op == FunctionVariableOperators.Multiply) value *= varOpPair.variable.Value;
            if(op == FunctionVariableOperators.Divide) value /= varOpPair.variable.Value;
            if(op == FunctionVariableOperators.Subtract) value -= varOpPair.variable.Value;
            op = varOpPair.op;
        }
        this.Value = value;
        OnUpdate?.Invoke();
    }

    private string Equation {
        get {
            if(VarOpPairs.Count == 0) return "Equation:";
            var eq = "Equation: v0 ";
            for(int i = 0; i < VarOpPairs.Count; i++) {
                if(i == VarOpPairs.Count - 1) break;
                if(VarOpPairs[i].op == FunctionVariableOperators.Add) eq += $"+ v{i+1} ";
                if(VarOpPairs[i].op == FunctionVariableOperators.Multiply) eq += $"* v{i+1} ";
                if(VarOpPairs[i].op == FunctionVariableOperators.Divide) eq += $"/ v{i+1} ";
                if(VarOpPairs[i].op == FunctionVariableOperators.Subtract) eq += $"- v{i+1} ";
            }
            return eq;
        }
    }
}

[Serializable]
public class VariableOperatorPair
{
    [HideLabel]
    public FloatReference variable;
    
    [HideLabel]
    public FunctionVariableOperators op;
}

public enum FunctionVariableOperators {
    Add,
    Multiply,
    Divide,
    Subtract
}
