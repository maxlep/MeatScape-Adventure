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
    public class MixerNode : AnimationStateNode
    {
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField]
        private BlendMode blendMode = BlendMode.Mix;
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField]
        private AnimationStateNode state1;
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), SerializeField]
        private AnimationStateNode state2;
        [HideIf("$collapsed"), LabelWidth(LABEL_WIDTH), PropertySpace(0f, 10f), SerializeField]
        private FloatReference factor;
        [HideIf("$collapsed"), HideLabel, BoxGroup("MixerRunner"), SerializeField]
        private MixerRunner mixerRunner;
        
        public override Playable Output => mixerRunner.Output;

        public override void RuntimeInitialize(int startNodeIndex)
        {
            base.RuntimeInitialize(startNodeIndex);
            
            mixerRunner.Initialize(base.name, sharedData);
            mixerRunner.Output.ConnectInput(0, state1.Output, 0);
            mixerRunner.Output.ConnectInput(1, state2.Output, 0);
            mixerRunner.Output.SetInputWeight(0, 1f);
            mixerRunner.Output.SetInputWeight(1, 1f);
            mixerRunner.BlendMode = blendMode;
        }

        public override void Execute()
        {
            base.Execute();

            mixerRunner.Factor = factor.Value;
            // Debug.Log($"Update walk mixer factor {mixerRunner.Factor}");
            mixerRunner.Update();
        }

        public override void OnApplictionExit()
        {
            base.OnApplictionExit();
            mixerRunner.Dispose();
        }
    }
}
