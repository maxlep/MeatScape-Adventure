using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    [Required]
    // [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    [CreateAssetMenu(fileName = "FloatVariable", menuName = "Variables/FloatVariable", order = 0)]
    public class FloatVariable : Variable
    {
        [SerializeField] private float defaultValue;
        [SerializeField] private float runtimeValue;
    
        public float Value
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
    public class FloatReference : Reference
    {
        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
        [SerializeField] private float ConstantValue;
    
        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")] 
        [SerializeField] private FloatVariable Variable;
    
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
                
                return (Variable != null) ? Variable.name : "<Missing Float>";
            }
        }

        public float Value
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
                        Debug.LogError("Trying to access Float variable but none set in inspector!");
                        return 0f;
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