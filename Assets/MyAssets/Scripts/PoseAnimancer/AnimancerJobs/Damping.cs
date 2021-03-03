using System;
using System.Collections.Generic;
using Animancer;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;

namespace MyAssets.Scripts.PoseAnimancer.AnimancerJobs
{
    public class Damping : AnimancerJob<DampingJob>, IDisposable
    {
#region Initialisation
        public Damping(AnimancerPlayable animancer, int boneCount, Transform endBone)
        {
            // Create the job and initialise all its arrays.
            // They are all Persistent because we want them to last for the full lifetime of the job.
            // Most of them can use UninitializedMemory which is faster because we will be immediately filling them.
            // But the velocities will use the default ClearMemory to make sure all the values start at zero.

            Validate(animancer, boneCount, endBone);
            
            // Since we are about to use these values several times, we can shorten the following lines a bit by using constants:
            const Allocator Persistent = Allocator.Persistent;
            const NativeArrayOptions UninitializedMemory = NativeArrayOptions.UninitializedMemory;

            _Job = new DampingJob()
            {
                jointHandles = new NativeArray<TransformStreamHandle>(boneCount, Persistent, UninitializedMemory),
                localPositions = new NativeArray<Vector3>(boneCount, Persistent, UninitializedMemory),
                localRotations = new NativeArray<Quaternion>(boneCount, Persistent, UninitializedMemory),
                positions = new NativeArray<Vector3>(boneCount, Persistent, UninitializedMemory),
                velocities = new NativeArray<Vector3>(boneCount, Persistent),
            };

            // Initialise the contents of the arrays for each bone.
            var animator = animancer.Component.Animator;
            var bone = endBone;
            for (int i = boneCount - 1; i >= 0; i--)
            {
                _Job.jointHandles[i] = animator.BindStreamTransform(bone);
                _Job.localPositions[i] = bone.localPosition;
                _Job.localRotations[i] = bone.localRotation;
                _Job.positions[i] = bone.position;

                bone = bone.parent;
            }

            _Job.rootHandle = animator.BindStreamTransform(bone);

            // Add the job to Animancer's output.
            // animancer.InsertOutputJob(_Job);
            
            CreatePlayable(animancer);

            animancer.Disposables.Add(this);
        }
        
        /// <summary>
        /// Ensures that the <see cref="_BoneCount"/> is positive and not larger than the number of bones between the
        /// <see cref="_EndBone"/> and the <see cref="Animator"/>.
        /// </summary>
        /// <remarks>
        /// Called by the Unity Editor in Edit Mode whenever an instance of this script is loaded or a value is changed
        /// in the Inspector.
        /// </remarks>
        private void Validate(AnimancerPlayable animancer, int boneCount, Transform endBone)
        {
            if (boneCount < 1)
            {
                boneCount = 1;
            }
            else if (endBone != null && animancer != null && animancer.Component.Animator != null)
            {
                var root = animancer.Component.Animator.transform;

                var bone = endBone;
                for (int i = 0; i < boneCount; i++)
                {
                    bone = bone.parent;
                    if (bone == root)
                    {
                        boneCount = i + 1;
                        break;
                    }
                    else if (bone == null)
                    {
                        endBone = null;
                        Debug.LogWarning("The End Bone must be a child of the Animator.");
                        break;
                    }
                }
            }
        }
        
#if !UNITY_2019_1_OR_NEWER
        private static bool _HasLoggedUnityVersionWarning;
        
        private void Start()
        {
            if (!_HasLoggedUnityVersionWarning && !_Animancer.Animator.isHuman)
            {
                _HasLoggedUnityVersionWarning = true;
                Debug.LogWarning("A bug in Unity versions older than 2019.1 prevents the Damping system from working on Generic Rigs." +
                    " The DampingJob relies on world positions but TransformStreamHandle.GetPosition returns local positions.", this);
            }
        }
#endif
        
#endregion

#region Clean Up
        void IDisposable.Dispose() => Dispose();

        /// <summary>Cleans up the <see cref="NativeArray{T}"/>s.</summary>
        /// <remarks>Called by <see cref="AnimancerPlayable.OnPlayableDestroy"/>.</remarks>
        private void Dispose()
        {
            if (_Job.jointHandles.IsCreated)
                _Job.jointHandles.Dispose();
            
            if (_Job.localPositions.IsCreated)
                _Job.localPositions.Dispose();
            
            if (_Job.localRotations.IsCreated)
                _Job.localRotations.Dispose();
            
            if (_Job.positions.IsCreated)
                _Job.positions.Dispose();

            if (_Job.velocities.IsCreated)
                _Job.velocities.Dispose();
        }

        /// <summary>Destroys the <see cref="_Playable"/> and restores the graph connection it was intercepting.</summary>
        public override void Destroy()
        {
            Dispose();
            base.Destroy();
        }
#endregion
    }
}