using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    // [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    [CreateAssetMenu(fileName = "BoolVariable", menuName = "Variables/BoolVariable", order = 0)]
    public class BoolVariable : Variable
    {
        [SerializeField] private bool defaultValue;
        [SerializeField] private bool runtimeValue;

        public bool Value
        {
            get => runtimeValue;
            set
            {
                runtimeValue = value;
                base.BroadcastUpdate();
            }
        }

        private void OnEnable() => runtimeValue = defaultValue;
    }

    [Serializable]
    [InlineProperty]
    public class BoolReference : Reference
    {
        [BoxGroup("Split/Right", ShowLabel = false)] [LabelText("Value")] [ShowIf("UseConstant")]
        [SerializeField] private bool ConstantValue = false;
    
        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")]
        [SerializeField] private BoolVariable Variable;
    
        public String Tooltip => Variable != null && !UseConstant ? Variable.Description : "";
        public String Name
        {
            get
            {
                if (UseConstant) 
                    return $"<Const>{ConstantValue}";
                
                return (Variable != null) ? Variable.name : "<Missing Bool>";
            }
        }

        public bool Value
        {
            get
            {
                if (Variable != null)
                    return UseConstant ? ConstantValue : Variable.Value;
                else
                {
                    if (UseConstant)
                        return ConstantValue;
                    else
                    {
                        Debug.LogError("Trying to access Bool variable but none set in inspector!");
                        return false;
                    }
                }
            }
            set
            {
                if (Variable != null) Variable.Value = value;
            } 
        }
    }
}