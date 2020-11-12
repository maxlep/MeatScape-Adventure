using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    // [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    [CreateAssetMenu(fileName = "QuaternionVariable", menuName = "Variables/QuaternionVariable", order = 0)]
    public class QuaternionVariable : Variable
    {
        [SerializeField] [LabelWidth(110f)] private Quaternion defaultValue;
        [SerializeField] [LabelWidth(110f)] private Quaternion runtimeValue;
    
        public Quaternion Value
        {
            get => runtimeValue;
            set
            {
                runtimeValue = value;
                base.BroadcastUpdate();
            }
        }

        public void Reset() => runtimeValue = defaultValue;

        private void OnEnable() => Reset();
    
    }

    [Serializable]
    [InlineProperty]
    public class QuaternionReference : Reference
    {
        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
        [SerializeField] private Quaternion ConstantValue;
    
        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")] 
        [SerializeField] private QuaternionVariable Variable;
    
        public String Tooltip => Variable != null && !UseConstant ? Variable.Description : "";
        
        public void Reset()
        {
            if (Variable != null && !UseConstant)
                Variable.Reset();
            else
                Debug.Log($"Trying to reset a SO <{Name}> using a constant value1 Nothing will happen.");
        } 
        
        public String Name
        {
            get
            {
                if (UseConstant) 
                    return $"<Const>{ConstantValue}";
                
                return (Variable != null) ? Variable.name : "<Missing Int>";
            }
        }

        public Quaternion Value
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
                        Debug.LogError("Trying to access Quaternion variable but none set in inspector!");
                        return Quaternion.identity;
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