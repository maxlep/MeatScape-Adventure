using System;
using System.Collections.Generic;
using System.Linq;
using MyAssets.Runtime.AnimationJobs;
using MyAssets.ScriptableObjects.Variables;
using MyAssets.Scripts.PoseAnimator.Types;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Debug = UnityEngine.Debug;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public class NewSequenceBlend : AnimationStateNode
    {
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField]
        private SequenceBlendRunner animationJobRunner;
        
        public override Playable Output => animationJobRunner.Output;

        public override void RuntimeInitialize(int startNodeIndex)
        {
            base.RuntimeInitialize(startNodeIndex);
            
            animationJobRunner.Initialize(base.name, sharedData);
        }

        public override void Execute()
        {
            base.Execute();
            
            animationJobRunner.Update();
        }
        
        private void OnDestroy()
        {
            animationJobRunner.Dispose();
        }
    }
}
