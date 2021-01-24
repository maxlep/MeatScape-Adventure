using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class StateChangeEventListener : MonoBehaviour
{
    [SerializeField] [Required] private StateMachineGraph StateMachine;

    [Required("No value set for ExitState, ExitState check will be ignored", InfoMessageType.Warning)]
    [SerializeField] private StateNode ExitState;

    [Required("No value set for EnterState, EnterState check will be ignored", InfoMessageType.Warning)]
    [SerializeField] private StateNode EnterState;

    public UnityEvent OnStateChange;

    private void Start()
    {
        StateMachine.onChangeState += EvaluateStateChange;
    }

    private void EvaluateStateChange(StateNode exitingState, StateNode enteringState)
    {
        if (EnterState == null && ExitState == null)
            return;

        if (EnterState != null && enteringState != EnterState)
            return;
        
        if (ExitState != null && exitingState != ExitState)
            return;
        
        OnStateChange.Invoke();
    }
}
