using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class ValidStateEventListener : MonoBehaviour
{
    [SerializeField] [Required] 
    private List<StateMachineGraph> StateMachines;

    [Required] [Tooltip("Event is fired when ANY of these states are entered")]
    [SerializeField] 
    private List<StateNode> ValidStates;

    [Tooltip("Event is fired when ANY of the above states are entered")]
    [PropertySpace(10f, 0f)]
    public UnityEvent OnEnterValidState;

    private void Start()
    {
        foreach (var stateMachine in StateMachines)
        {
            stateMachine.onChangeState += EvaluateStateChange;
        }
    }

    private void EvaluateStateChange(StateNode exitingState, StateNode enteringState)
    {
        if (ValidStates.Contains(enteringState))
            OnEnterValidState.Invoke();
    }
}
