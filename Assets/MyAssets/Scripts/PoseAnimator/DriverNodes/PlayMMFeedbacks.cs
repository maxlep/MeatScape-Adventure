using System;
using MyAssets.ScriptableObjects.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.Scripts.PoseAnimator.AnimationNodes
{
    public class PlayMMFeedbacks : StateNode
    {
        [HideIf("$zoom"), LabelWidth(LABEL_WIDTH), SerializeField] private MMFeedbacksSceneReference feedbacks;

        public override void Enter()
        {
            base.Enter();
            
            feedbacks.Value.PlayFeedbacks();
        }
    }
}