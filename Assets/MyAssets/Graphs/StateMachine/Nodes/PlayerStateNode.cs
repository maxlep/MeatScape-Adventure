using UnityEngine;


public class PlayerStateNode : StateNode
{
    protected PlayerController playerController;
    
    public void SetDependencies(PlayerController playerController)
    {
        this.playerController = playerController;
    }

}
