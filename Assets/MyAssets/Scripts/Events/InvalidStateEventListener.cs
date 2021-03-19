using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

public class InvalidStateEventListener : MonoBehaviour
{
    
    [SerializeField] [Required] 
    private List<StateMachineGraph> StateMachines;

    [Required] [Tooltip("Event is fired when NONE of the states above are active and we just exited one or more of them")]
    [SerializeField] 
    private List<StateNode> InvalidStates;

    [Tooltip("Event is fired when NONE of the states above are active and we just exited one or more of them")]
    [PropertySpace(10f, 0f)]
    public UnityEvent OnNoInvalidStateActive;

    private void Start()
    {
        foreach (var stateMachine in StateMachines)
        {
            stateMachine.onChangeState += EvaluateStateChange;
        }
    }

    private void EvaluateStateChange(StateNode exitingState, StateNode enteringState)
    {
        //If not exiting one of these states, return (to prevent this firing event nonstop when none are active)
        if (!InvalidStates.Contains(exitingState)) return;
        
        //Return if any invalid state is active
        foreach (var stateMachine in StateMachines)
            foreach (var invalidState in InvalidStates)
                if (stateMachine.currentStates.Contains(invalidState)) return;
            
        

        OnNoInvalidStateActive.Invoke();
    }
}
