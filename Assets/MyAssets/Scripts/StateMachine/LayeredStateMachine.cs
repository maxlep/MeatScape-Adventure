using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using Sirenix.Utilities;

public class LayeredStateMachine : MonoBehaviour
{
    [SerializeField] protected StateMachineGraph[] stateMachines;
    [SerializeField] protected VariableContainer debugParameters;

    protected Dictionary<StateNode, StateMachineGraph> stateNodeDict = new Dictionary<StateNode, StateMachineGraph>();


    #region LifeCycle Methods

    protected virtual void Awake()
    {
        InitStateMachines(true);
        stateMachines.ForEach(s => s.Awaken());
    }

    protected virtual void Update()
    {
        HandleTransitions();
        ExecuteUpdates();
    }

    protected virtual void FixedUpdate()
    {
        ExecuteFixedUpdates();
    }
    
    protected virtual void ExecuteUpdates()
    {
        stateMachines.ForEach(s => s.ExecuteUpdates());
    }
    
    protected virtual void ExecuteFixedUpdates()
    {
        stateMachines.ForEach(s => s.ExecuteFixedUpdates());
    }

    private void OnValidate()
    {
        stateMachines.ForEach(s => s.InjectDependencies(this));
    }

    protected void OnDrawGizmos()
    {
        stateMachines.ForEach(s => s.DrawGizmos());
    }

    protected void OnApplicationQuit()
    {
        stateMachines.ForEach(s => s.OnApplicationExit());
    }

    #endregion

    #region Init/Dep Injection

    [GUIColor(0, 1, 0)]
    [Button(ButtonSizes.Large, Name = "InitStateMachines")]
    public virtual void InitStateMachinesEditor()
    {
        InitStateMachines(false);
        stateMachines.ForEach(i => i.PopulateNodeLists());
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
    

    #endregion

    //Send requesting state machine list of active states
    //For transition node valid start states
    public virtual List<StateNode> GetActiveStates(StateMachineGraph requestingStateMachine)
    {
        List<StateNode> activeStates = new List<StateNode>();
        foreach (var stateMachine in stateMachines)
        {
            foreach (var currentState in stateMachine.currentStates)
            {
                activeStates.Add(currentState);
            }
        }

        return activeStates;
    }

    private void HandleTransitions()
    {
        foreach (var stateMachine in stateMachines)
        {
            stateMachine.CheckForValidTransitions();
        }
        foreach (var stateMachine in stateMachines)
        {
            stateMachine.ApplyValidTransitions();
        }
    }

    //Return true if invalid state is in any of its state machine's active states
    public virtual bool CheckInvalidStateActive(List<StateNode> invalidStates)
    {
        Dictionary<StateNode, StateMachineGraph> invalidStateDict  = new Dictionary<StateNode, StateMachineGraph>();
        
        //Populate dict of statemachine and its invalid state
        invalidStates.ForEach(s =>
        {
            if (!stateNodeDict.ContainsKey(s))
                Debug.LogError($"Trying to check invalid start state, {s.name} ,that is not part of state machines!");
            if (!invalidStateDict.ContainsKey(s)) 
                invalidStateDict.Add(s, stateNodeDict[s]);
        });

        bool result = false;
        
        //For each statemachine, check that at least 1 (OR) invalid state is active
        foreach (var stateMachine in stateMachines)
        {
            bool isInvalidStateActive = false;
            foreach (var pair in invalidStateDict)
            {
                if (pair.Value == stateMachine)
                {
                    if (stateMachine.currentStates.Contains(pair.Key))
                    {
                        isInvalidStateActive = true;
                        break;
                    }
                }
            }

            result |= isInvalidStateActive;
        }

        return result;
    }

    #region Debug

    private void OnGUI()
    {
        if (!DebugManager.Instance.EnableDebugGUI) return;

        DrawStateMachineDebug();
        DrawDebugParameters();
    }

    private void DrawStateMachineDebug()
    {
        //Current States
        float pivotX = 10f;
        float pivotY = 80f;
        float width = 300f;
        float lineHeight = 20f;
        float verticalMargin = 0f;
        float betweenStatesMargin = 5f;
        
        float previousHeights = 0f;

        for (int i = 0; i < stateMachines.Length; i++)
        {

            string currentStatesString = "";
            GUI.TextField(new Rect(pivotX, pivotY + previousHeights + i*verticalMargin, width/2, lineHeight),
                $"{stateMachines[i].name}: \n");
            
            bool debugOnStateChange = stateMachines[i].DebugOnStateChange;
            if (GUI.Button(new Rect(pivotX + 150f, pivotY + previousHeights + i * verticalMargin, width/2, lineHeight),
                "PauseOnChange: " + (debugOnStateChange ? "YES" : "NO"))) stateMachines[i].ToggleDebugOnStateChange();

            previousHeights += lineHeight;
            
            foreach (var currentState in stateMachines[i].currentStates)
            {
                
                currentStatesString += $"- {currentState.name}\n";
            }

            int lineCount = stateMachines[i].currentStates.Count;
            float height = lineCount * lineHeight;

            GUI.TextField(new Rect(pivotX, pivotY + previousHeights + i*verticalMargin, width, height),
                $"{currentStatesString}");
            
            previousHeights += height + betweenStatesMargin;
        }
    }

    private void DrawDebugParameters()
    {
        float width = 400f;
        float pivotX = Screen.width - width - 10f;
        float pivotY = 10f;
        float lineHeight = 20f;
        float betweenTypeMargin = 15f;
        
        float previousHeights = 0f;
        
        void DrawDebugList(string title, List<string> listItems)
        {
            GUI.TextField(new Rect(pivotX, pivotY + previousHeights, width, lineHeight),
                $"{title}: \n");
            previousHeights += lineHeight;
            
            string listContent = string.Join("\n", listItems.Select(i => $"- {i}"));
            float listSize = (lineHeight) * listItems.Count();
            
            GUI.TextField(new Rect(pivotX, pivotY + previousHeights, width, listSize),
                listContent);
            previousHeights += listSize + betweenTypeMargin;
        }
        
        DrawDebugList(
            "Bool Variables",
            debugParameters.GetBoolVariables()
                .Select(v => $"{v.name}: {v.Value}")
                .ToList()
        );
        
        DrawDebugList(
            "Int Variables",
            debugParameters.GetIntVariables()
                .Select(v => $"{v.name}: {v.Value}")
                .ToList()
        );
        
        DrawDebugList(
            "Float Variables",
            debugParameters.GetFloatVariables()
                .Select(v => $"{v.name}: {v.Value}")
                .ToList()
        );
        
        DrawDebugList(
            "Vector2 Variables",
            debugParameters.GetVector2Variables()
                .Select(v => $"{v.name}: {v.Value}")
                .ToList()
        );
        
        DrawDebugList(
            "Vector3 Variables",
            debugParameters.GetVector3Variables()
                .Select(v => $"{v.name}: {v.Value}")
                .ToList()
        );
        
        DrawDebugList(
            "Quaternion Variables",
            debugParameters.GetQuaternionVariables()
                .Select(v => $"{v.name}: {v.Value}")
                .ToList()
        );
        
        DrawDebugList(
            "Timer Variables",
            debugParameters.GetTimerVariables()
                .Select(v => $"{v.name}: {v.RemainingTime}")
                .ToList()
        );
        
        DrawDebugList(
            "Function Variables",
            debugParameters.GetFunctionVariables()
                .Select(v => $"{v.name}: {v.Value}")
                .ToList()
        );
    }
    

    #endregion

}