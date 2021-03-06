﻿using MyAssets.ScriptableObjects.Variables;
using MyAssets.ScriptableObjects.Variables.ValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;
using MyAssets.Scripts.Utils;
using Shapes;
using Sirenix.Utilities;

public class ForwardAttack : PlayerStateNode
{
    #region Inputs

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private MeatClumpController MeatClumpPrefab;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private IntReference CurrentHungerLevel;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required] 
    private IntReference HungerCost;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private FloatReference ThrowDelay;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required] 
    private FloatReference ThrowSpeed;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required]
    private BoolReference ClumpOverCharged;

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private TransformSceneReference FirePoint;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private TransformSceneReference PlayerCameraTransform;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private Vector2Reference MoveInput;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required] 
    private TimerReference comboTimer;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField]
    [TabGroup("Inputs")] [Required] 
    private TimerReference comboFinishTimer;
    
    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Inputs")] [Required] 
    private TransformSceneReference CurrentTargetRef;
    
    #endregion

    #region Outputs

    [HideIf("$collapsed")] [LabelWidth(LABEL_WIDTH)] [SerializeField] 
    [TabGroup("Outputs")] [Required] 
    private BoolReference WaitedAttackDelay;

    #endregion

    private bool clumpThrown;

    public override void Initialize(StateMachineGraph parentGraph)
    {
        base.Initialize(parentGraph);
    }

    private Vector3 GetMoveDirection()
    {
        Vector2 camForward = PlayerCameraTransform.Value.forward.xz().normalized;
        Quaternion rotOffset = Quaternion.FromToRotation(Vector2.up, camForward);
        Vector2 rotatedMoveInput = rotOffset * MoveInput.Value;
        return new Vector3(rotatedMoveInput.x, 0, rotatedMoveInput.y);
    }
    
    public override void Enter()
    {
        base.Enter();
        
        comboTimer.RestartTimer();
        comboFinishTimer.RestartTimer();
        CurrentHungerLevel.Value = Mathf.FloorToInt(Mathf.Max(0f, CurrentHungerLevel.Value - HungerCost.Value));

        Vector3 throwDirection;
        var aimTarget = CurrentTargetRef.Value;
        
        //If no target, throw forward
        if (aimTarget.SafeIsUnityNull())
        {
            throwDirection = GetMoveDirection().normalized;
        }
        //If find collider, target center bounds, otherwise target transform.
        else
        {
            Collider targetCollider = aimTarget.GetComponent<Collider>();
            Vector3 targetPosition;

            if (targetCollider != null)
                targetPosition = targetCollider.bounds.center;
            

            else
                targetPosition = aimTarget.position;
            

            throwDirection = (targetPosition - FirePoint.Value.position).normalized;
        }

        if(Mathf.Approximately(throwDirection.magnitude, 0)) throwDirection = FirePoint.Value.forward;

        MeatClumpController clump = Instantiate(MeatClumpPrefab);
        clump.transform.position = FirePoint.Value.position;
        clump.SetMoving(ThrowSpeed.Value, throwDirection);
        if (ClumpOverCharged.Value) clump.SetOverCharged();

        playerController.OnClumpThrown(throwDirection);
    }

    public override void Execute()
    {
        base.Execute();
    }

    public override void ExecuteFixed()
    {
        base.ExecuteFixed();
    }


    public override void Exit()
    {
        base.Exit();

        WaitedAttackDelay.Value = false;
        
        TimeUtils.SetTimeout(ThrowDelay.Value, () => WaitedAttackDelay.Value = true);
    }
}