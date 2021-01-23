using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using MyAssets.ScriptableObjects.Variables;
using System;

[Required]
[CreateAssetMenu(fileName = "FunctionVariable", menuName = "Variables/FunctionVariable", order = 0)]
public class FunctionVariable : ScriptableObject
{
    [Title("$Equation")]
    [HideReferenceObjectPicker][SerializeField] private List<VariableOperatorPair> VarOpPairs;

    public float Value {get; private set;}

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
public class VariableOperatorPair {
    public FloatReference variable;
    public FunctionVariableOperators op;
}

public enum FunctionVariableOperators {
    Add,
    Multiply,
    Divide,
    Subtract
}
