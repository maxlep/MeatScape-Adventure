﻿using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    public delegate void OnUpdate();
    
    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    // [CreateAssetMenu(fileName = "Variable", menuName = "Variables/Variable", order = 0)]
    public class Variable : ScriptableObject
    {
        [TextArea] [HideInInlineEditors] public String Description;

        public event OnUpdate OnUpdate;

        public void Subscribe(OnUpdate callback)
        {
            OnUpdate += callback;
        }
        
        protected void BroadcastUpdate()
        {
            OnUpdate?.Invoke();
        }
    }
    
    [Serializable]
    [InlineProperty]
    public class Reference
    {
        [HorizontalGroup("Split", LabelWidth = .01f)] [PropertyTooltip("$Tooltip")]
        [BoxGroup("Split/Left", ShowLabel = false)] [LabelText("$LabelText")] [LabelWidth(10f)]
        [SerializeField] protected bool UseConstant = false;
        
        public String LabelText => UseConstant ? "" : "?";

        protected event OnUpdate OnUpdate;

        public void Subscribe(OnUpdate callback)
        {
            OnUpdate += callback;
        }
    }
}