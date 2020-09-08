using System;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;
using System.Linq;
using System.Net.Configuration;
using System.Runtime.ConstrainedExecution;
using Sirenix.OdinInspector;

public class LayeredStateMachine : MonoBehaviour
{
    [SerializeField] protected StateMachineGraph[] stateMachines;

    protected Dictionary<StateNode, StateMachineGraph> stateNodeDict = new Dictionary<StateNode, StateMachineGraph>();
    private bool enableDebugGUI = false;


    #region LifeCycle Methods

    protected virtual void Awake()
    {
        InitStateMachines(true);
        InputManager.Instance.onEnableDebug += ToggleDebugMode;
    }

    protected virtual void Update()
    {
        ExecuteUpdates();
    }

    protected virtual void FixedUpdate()
    {
        ExecuteFixedUpdates();
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

    protected void OnDrawGizmos()
    {
        foreach (var stateMachine in stateMachines)
        {
            stateMachine.DrawGizmos();
        }
    }

    #endregion

    #region Init/Dep Injection

    [GUIColor(0, 1, 0)]
    [Button(ButtonSizes.Large, Name = "InitStateMachines")]
    public virtual void InitStateMachinesEditor()
    {
        InitStateMachines(false);
    }

    
    public virtual void InitStateMachines(bool isRuntime)
    {
        stateNodeDict.Clear();
        
        //Loop through and init nodes
        foreach (var stateMachine in stateMachines)
        {
            stateMachine.InjectDependencies(this);
            stateMachine.PopulateNodeLists();
            PopulateStateNodeDict(stateMachine);
            InjectNodeDependencies(stateMachine);
        }

        //Actually start machine and send over state nodes dict from other machines
        foreach (var stateMachine in stateMachines)
        {
            SendValidStartStates(stateMachine);
            stateMachine.StartStateMachine(isRuntime);
        }
        
        Debug.Log("Finished Initializing State Machines");
    }

    protected virtual void InjectNodeDependencies(StateMachineGraph stateMachine)
    {

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
        
        receivingStateMachine.SendValidStatesToTransitions(otherStateNodes);
    }

    #endregion

    //Send requesting state machine list of active states in the other state machines
    //For transition node valid start states
    public virtual List<StateNode> GetActiveStates(StateMachineGraph requestingStateMachine)
    {
        List<StateNode> activeStates = new List<StateNode>();
        foreach (var stateMachine in stateMachines)
        {
            if (stateMachine != requestingStateMachine)
            {
                foreach (var currentState in stateMachine.currentStates)
                {
                    activeStates.Add(currentState);
                }
                
            }
        }

        return activeStates;
    }

    #region Debug
    
    private void ToggleDebugMode()
    {
        enableDebugGUI = !enableDebugGUI;
    }

    private void OnGUI()
    {
        if (!enableDebugGUI) return;
        
        //Current States
        float pivotX = 10f;
        float pivotY = 80f;
        float width = 300f;
        float lineHeight = 20f;
        float verticalMargin = 0f;
        
        float previousHeights = 0f;

        for (int i = 0; i < stateMachines.Length; i++)
        {
            
            string currentStatesString = $"{stateMachines[i].name}: \n";
            foreach (var currentState in stateMachines[i].currentStates)
            {
                currentStatesString += $"- {currentState.name}\n";
            }

            int lineCount = stateMachines[i].currentStates.Count + 1;
            float height = lineCount * lineHeight;

            GUI.TextField(new Rect(pivotX, pivotY + previousHeights + i*verticalMargin, width, height),
                $"{currentStatesString}");

            previousHeights += height;
        }
        
    }

    #endregion

}