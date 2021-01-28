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

    protected void OnApplicationQuit()
    {
        foreach (var stateMachine in stateMachines)
        {
            stateMachine.OnApplicationExit();
        }
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

        //Bools
        GUI.TextField(new Rect(pivotX, pivotY + previousHeights, width, lineHeight),
            $"Bool Variables: \n");
        previousHeights += lineHeight;
        
        string boolString = "";
        foreach (var boolVariable in debugParameters.GetBoolVariables())
        {
            boolString += $"- {boolVariable.name}: {boolVariable.Value} \n";
        }

        float boolSize = (lineHeight) * debugParameters.GetBoolVariables().Count;
        
        GUI.TextField(new Rect(pivotX, pivotY + previousHeights, width, boolSize),
            boolString);
        previousHeights += boolSize + betweenTypeMargin;
        
        //Ints
        GUI.TextField(new Rect(pivotX, pivotY + previousHeights, width, lineHeight),
            $"Int Variables: \n");
        previousHeights += lineHeight;
        
        string intString = "";
        foreach (var intVariable in debugParameters.GetIntVariables())
        {
            intString += $"- {intVariable.name}: {intVariable.Value} \n";
        }
        
        float intSize = (lineHeight) * debugParameters.GetIntVariables().Count;


        GUI.TextField(new Rect(pivotX, pivotY + previousHeights, width, intSize),
            intString);
        previousHeights += intSize + betweenTypeMargin;
        
        //Floats
        GUI.TextField(new Rect(pivotX, pivotY + previousHeights, width, lineHeight),
            $"Float Variables: \n");
        previousHeights += lineHeight;
        
        string floatString = "";
        foreach (var floatVariable in debugParameters.GetFloatVariables())
        {
            floatString += $"- {floatVariable.name}: {floatVariable.Value} \n";
        }
        
        float floatSize = (lineHeight) * debugParameters.GetFloatVariables().Count;


        GUI.TextField(new Rect(pivotX, pivotY + previousHeights, width, floatSize),
            floatString);
        previousHeights += floatSize + betweenTypeMargin;
        
        //Vector2
        GUI.TextField(new Rect(pivotX, pivotY + previousHeights, width, lineHeight),
            $"Vector2 Variables: \n");
        previousHeights += lineHeight;
        
        string vector2String = "";
        foreach (var vector2Variable in debugParameters.GetVector2Variables())
        {
            vector2String += $"- {vector2Variable.name}: {vector2Variable.Value} \n";
        }
        
        float vector2Size = (lineHeight) * debugParameters.GetVector2Variables().Count;


        GUI.TextField(new Rect(pivotX, pivotY + previousHeights, width, vector2Size),
            vector2String);
        previousHeights += vector2Size + betweenTypeMargin;
        
        //Vector3
        GUI.TextField(new Rect(pivotX, pivotY + previousHeights, width, lineHeight),
            $"Vector3 Variables: \n");
        previousHeights += lineHeight;
        
        string vector3String = "";
        foreach (var vector3Variable in debugParameters.GetVector3Variables())
        {
            vector3String += $"- {vector3Variable.name}: {vector3Variable.Value} \n";
        }
        
        float vector3Size = (lineHeight) * debugParameters.GetVector3Variables().Count;


        GUI.TextField(new Rect(pivotX, pivotY + previousHeights, width, vector3Size),
            vector3String);
        previousHeights += vector3Size + betweenTypeMargin;
        
        //Quaternion
        GUI.TextField(new Rect(pivotX, pivotY + previousHeights, width, lineHeight),
            $"Quaternion Variables: \n");
        previousHeights += lineHeight;
        
        string quaternionString = "";
        foreach (var quaternionVariable in debugParameters.GetQuaternionVariables())
        {
            quaternionString += $"- {quaternionVariable.name}: {quaternionVariable.Value} \n";
        }
        
        float quaternionSize = (lineHeight) * debugParameters.GetQuaternionVariables().Count;


        GUI.TextField(new Rect(pivotX, pivotY + previousHeights, width, quaternionSize),
            quaternionString);
        previousHeights += quaternionSize + betweenTypeMargin;
        
        //Timer
        GUI.TextField(new Rect(pivotX, pivotY + previousHeights, width, lineHeight),
            $"Timer Variables: \n");
        previousHeights += lineHeight;
        
        string timerString = "";
        foreach (var timerVariable in debugParameters.GetTimerVariables())
        {
            timerString += $"- {timerVariable.name}: {timerVariable.RemainingTime} \n";
        }
        
        float timerSize = (lineHeight) * debugParameters.GetTimerVariables().Count;


        GUI.TextField(new Rect(pivotX, pivotY + previousHeights, width, timerSize),
            timerString);
        previousHeights += timerSize + betweenTypeMargin;
    }
    

    #endregion

}