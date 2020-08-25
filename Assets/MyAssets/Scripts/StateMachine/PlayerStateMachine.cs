using System;
using KinematicCharacterController;
using UnityEngine;


public class PlayerStateMachine : LayeredStateMachine
{
    [SerializeField] private PlayerController playerController;
    
    
    protected override void InjectNodeDependencies(StateMachineGraph stateMachine)
    {
        foreach (var stateNode in stateMachine.stateNodes)
        {
            var playerState = stateNode as PlayerStateNode;
            if (playerState != null)
            {
                playerState.SetDependencies(playerController);
            }
        }

        foreach (var transitionNode in stateMachine.transitionNodes)
        {
            transitionNode.parameters = parameters;
        }
    }
}
