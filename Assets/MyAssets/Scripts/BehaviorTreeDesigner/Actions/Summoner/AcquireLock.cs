using System.Collections;
using System.Collections.Generic;
using MyAssets.Scripts.Utils;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityRigidbody;
using Den.Tools;
using MoreMountains.Feedbacks;
using UnityEngine;
using Timer = System.Timers.Timer;

[TaskCategory("Combat")]
[TaskDescription("Acquires lock on target.")]
public class AcquireLock : Action
{
    public SharedTransform Targeter;
    public SharedTransform TargetingReticle;
    public SharedTransform Target;
    public SharedFloat TimeToLock = 1;
    public float LeadTime = 0.5f;
    
    public SharedTransform TargetingFeedbacksTransform;
    private MMFeedbacks _targetingFeedbacks;

    private float? lockStartTime;
    // private Collider targetCollider;
    // private Rigidbody targetRigidbody;

    public int Npositions = 30;
    private LinkedList<PositionLog> positions;

    private struct PositionLog
    {
        public Vector3 position;
        public float time;

        public PositionLog(Vector3 position, float time)
        {
            this.position = position;
            this.time = time;
        }
    }

    public override void OnAwake()
    {
        positions = new LinkedList<PositionLog>();
    }
    
    public override void OnStart()
    {
        lockStartTime = Time.time;
        // targetCollider = Target.Value.GetComponent<Collider>();
        // targetRigidbody = Target.Value.GetComponent<Rigidbody>();

        _targetingFeedbacks = TargetingFeedbacksTransform.Value.GetComponent<MMFeedbacks>();
        _targetingFeedbacks.PlayFeedbacks();
    }
    
    public override void OnEnd()
    {
        _targetingFeedbacks.StopFeedbacks();
    }

    public override TaskStatus OnUpdate()
    {
        var elapsedTime = Time.time - lockStartTime.Value;
        
        var pct = elapsedTime / TimeToLock.Value;
        var reticlePos = TargetingReticle.Value.position;
        var targetPos = Target.Value.position;

        LogPosition(targetPos);

        var average = GetAverageOfLogs();
        var leadOffset = average.velocity * LeadTime;
        leadOffset.y = 0;
        var predictedTargetPos = average.position + leadOffset;
        predictedTargetPos.y = targetPos.y;
        // var diff = predictedTargetPos - reticlePos;
        // var pos = Vector3.MoveTowards(reticlePos, predictedTargetPos, diff.sqrMagnitude / 8f);
        // var didHit = Physics.Raycast(predictedTargetPos.xoz() + Vector3.up * (targetPos.z + 50), Vector3.down, out RaycastHit hitInfo, 100, LayerMask.NameToLayer("Terrain"));
        // var pos = didHit ? hitInfo.point : predictedTargetPos;
        var pos = predictedTargetPos;

        // pct = 0;
        pos = Vector3.Lerp(pos, targetPos + leadOffset, pct);

        pos = targetPos;
        var rot = TargetingReticle.Value.rotation;
        // TargettingReticle.Value.SetPositionAndRotation(pos, rot);
        TargetingReticle.Value.SetPositionAndRotation(pos, rot);

        var targeterPos = Targeter.Value.position;
        var dir = pos - targeterPos;
        dir.y = 0;
        Targeter.Value.SetPositionAndRotation(targeterPos, Quaternion.LookRotation(dir, Targeter.Value.up));
        
        if (elapsedTime < TimeToLock.Value)
        {
            return TaskStatus.Running;
        }
        
        return TaskStatus.Success;
    }
    
    public override void OnReset()
    {
        lockStartTime = null;
        Target.Value = null;
        positions.Clear();
        // targetCollider = null;
        // targetRigidbody = null;
    }

    private void LogPosition(Vector3 position)
    {
        positions.AddFirst(new PositionLog(position, Time.time));
        while (positions.Count > Npositions)
        {
            positions.RemoveLast();
        }
    }

    private (Vector3 position, Vector3 velocity, float time) GetAverageOfLogs()
    {
        Vector3 averagePosition = Vector3.zero;
        Vector3 averageVelocity = Vector3.zero;
        var curr = positions.First;
        while (curr != null)
        {
            averagePosition += curr.Value.position;
            averageVelocity += (curr.Value.position - (curr.Next?.Value.position ?? curr.Value.position))
                               / (curr.Value.time - (curr.Next?.Value.time ?? curr.Value.time));
            
            curr = curr.Next;
        }
        averagePosition /= positions.Count;
        averageVelocity /= positions.Count;

        float averageTime = GetTotalLogTime() / positions.Count;

        return (averagePosition, averageVelocity, averageTime);
    }

    private float GetTotalLogTime()
    {
        var start = positions.Last.Value.time;
        var end = positions.First.Value.time;
        return end - start;
    }
}
