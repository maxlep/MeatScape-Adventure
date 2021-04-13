using Sirenix.OdinInspector;
using UnityEngine;
using XNode;


public class AnyStateNode : Node
{

    [Output(typeConstraint = TypeConstraint.Strict)]  [PropertyOrder(-3)]  public StateMachineConnection Transitions;
    
}
