﻿using System;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using Sirenix.OdinInspector;

public class LayeredStateMachine : MonoBehaviour
{
    [SerializeField] protected StateMachineGraph[] stateMachines;
    [SerializeField] protected StateMachineParameters parameters;

    protected Dictionary<StateNode, StateMachineGraph> stateNodeDict = new Dictionary<StateNode, StateMachineGraph>();

    protected virtual void Awake()
    {
        InitStateMachines();
    }

    protected virtual void Update()
    {
        ExecuteUpdates();
    }

    protected virtual void FixedUpdate()
    {
        ExecuteFixedUpdates();
    }

    [GUIColor(0, 1, 0)]
    [Button(ButtonSizes.Large)]
    protected virtual void InitStateMachines()
    {
        stateNodeDict.Clear();
        
        //Loop through and init nodes
        foreach (var stateMachine in stateMachines)
        {
            stateMachine.SetParentMachine(this);
            stateMachine.PopulateNodeLists();
            PopulateStateNodeDict(stateMachine);
            InjectNodeDependencies(stateMachine);
        }

        //Actually start machine and send over state nodes dict from other machines
        foreach (var stateMachine in stateMachines)
        {
            SendValidStartStates(stateMachine);
            stateMachine.StartStateMachine();
        }
        
        Debug.Log("Finished Initializing State Machines");
    }

    protected virtual void InjectNodeDependencies(StateMachineGraph stateMachine)
    {
        foreach (var stateNode in stateMachine.stateNodes)
        {
            stateNode.SetParameters(parameters);
        }

        foreach (var transitionNode in stateMachine.transitionNodes)
        {
            transitionNode.SetParameters(parameters);
        }
    }

    protected virtual void ExecuteUpdates()
    {
        foreach (var stateMachine in stateMachines)
        {
            stateMachine.ExecuteUpdates();
        }
    }
    
    protected virtual void ExecuteFixedUpdates()
    {
        foreach (var stateMachine in stateMachines)
        {
            stateMachine.ExecuteFixedUpdates();
        }
    }

    //Populate dictionary of State Nodes and their respective State Machine
    protected virtual void PopulateStateNodeDict(StateMachineGraph stateMachine)
    {
        foreach (var stateNode in stateMachine.stateNodes)
        {
            stateNodeDict.Add(stateNode, stateMachine);
        }
    }

    //Get list of all states that are not for this machine
    //Send list over to the state machine so it can then send to transition nodes
    protected virtual void SendValidStartStates(StateMachineGraph receivingStateMachine)
    {
        List<StateNode> otherStateNodes = new List<StateNode>();
        foreach (var stateNode in stateNodeDict.Keys)
        {
            if (stateNodeDict[stateNode] != receivingStateMachine)
            {
                otherStateNodes.Add(stateNode);
            }
        }
        
        Debug.Log($"Sending {receivingStateMachine} {otherStateNodes.Count} stateNodes for dropdown");
        receivingStateMachine.SendValidStatesToTransitions(otherStateNodes);
    }

    //Send requesting state machine list of active states in the other state machines
    //For transition node valid start states
    public virtual List<StateNode> GetActiveStates(StateMachineGraph requestingStateMachine)
    {
        List<StateNode> activeStates = new List<StateNode>();
        foreach (var stateMachine in stateMachines)
        {
            if (stateMachine != requestingStateMachine)
            {
                activeStates.Add(stateMachine.currentState);
            }
        }

        return activeStates;
    }
    
    //Force each state machine to re-evaluate transitions
    protected virtual void ForceCheckForTransitions()
    {
        foreach (var stateMachine in stateMachines)
        {
            stateMachine.CheckForValidTransitions();
        }
    }
    
    //Send trigger down to state machines, then re-evaluate transitions
    public virtual void ActivateTrigger(string name)
    {
        if (parameters.TriggerParameters.ContainsKey(name))
        {
            parameters.TriggerParameters[name] = true;
            ForceCheckForTransitions();
            parameters.TriggerParameters[name] = false;
        }
    }

}