using System;
using KinematicCharacterController;
using Sirenix.OdinInspector;
using UnityEngine;


public class PlayerStateMachine : LayeredStateMachine
{
    [SerializeField] [SceneObjectsOnly] private PlayerController playerController;

    
    
    protected override void InjectNodeDependencies(StateMachineGraph stateMachine)
    {
        base.InjectNodeDependencies(stateMachine);

        foreach (var stateNode in stateMachine.stateNodes)
        {
            if (stateNode is PlayerStateNode playerState)
            {
                playerState.SetDependencies(playerController);
            }
            
            //Init playerStateNodes within subStateProcessor
            if (stateNode is SubStateProcessorNode nodeAsSubState)
            {
                foreach (var subState in nodeAsSubState.SubStates)
                {
                    if (subState is PlayerStateNode playerSubState)
                    {
                        playerSubState.SetDependencies(playerController);
                    }
                }
            }
        }
    }
}
