using System;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables
{
    public delegate void OnUpdate();
    public delegate void OnUpdate<T>(T previousValue, T currentValue);

    [Serializable]
    [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
    public class Variable<T> : ScriptableObject {
        [TextArea (7, 10)] [HideInInlineEditors] public String Description;
        [SerializeField] private T defaultValue;
        [SerializeField] private T runtimeValue;

        protected event OnUpdate OnUpdate;
        protected event OnUpdate<T> OnUpdateDelta;
    
        public T Value
        {
            get => runtimeValue;
            set
            {
                this.OnUpdateDelta?.Invoke(runtimeValue, value);
                runtimeValue = value;
                this.OnUpdate?.Invoke();
            }
        }

        public void Subscribe(OnUpdate callback)
        {
            this.OnUpdate += callback;
        }
        
        public void Subscribe(OnUpdate<T> callback)
        {
            this.OnUpdateDelta += callback;
        }

        public void Unsubscribe(OnUpdate callback)
        {
            this.OnUpdate -= callback;
        }
        
        public void Unsubscribe(OnUpdate<T> callback)
        {
            this.OnUpdateDelta -= callback;
        }

        public void Reset() => runtimeValue = defaultValue;

        private void OnEnable() => Reset();
        private void OnDisable() => OnUpdate = null;
    }

    [Serializable]
    [InlineProperty]
    [SynchronizedHeader]
    public class Reference<T, VT> where VT: Variable<T>
    {
        [HorizontalGroup("Split", LabelWidth = .01f)] [PropertyTooltip("$Tooltip")]
        [BoxGroup("Split/Left", ShowLabel = false)] [LabelText("$LabelText")] [LabelWidth(10f)]
        [SerializeField] protected bool UseConstant = false;
        
        public String LabelText => UseConstant ? "" : "?";

        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [ShowIf("UseConstant")]
        [SerializeField] protected T ConstantValue;
        
        [BoxGroup("Split/Right", ShowLabel = false)] [HideLabel] [HideIf("UseConstant")] 
        [SerializeField] protected VT Variable;
    
        public String Tooltip => Variable != null && !UseConstant ? $"{Variable.name}:\n{Variable.Description}" : "";

        //WARNING: will update subscribers synchronously within whatever time cycle the var is updated (Awake, LateUpdate, etc.)
        public void Subscribe(OnUpdate callback)
        {
            Variable?.Subscribe(callback);
        }

        public void Unsubscribe(OnUpdate callback)
        {
            Variable?.Unsubscribe(callback);
        }
        
        public void Subscribe(OnUpdate<T> callback)
        {
            Variable?.Subscribe(callback);
        }

        public void Unsubscribe(OnUpdate<T> callback)
        {
            Variable?.Unsubscribe(callback);
        }
        
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

        public T Value
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
                        Debug.LogError($"Trying to access Float variable but none set in inspector!");
                        return default(T);
                    }
                }
            }
            set
            {
                if (Variable != null) Variable.Value = value;
                else if (!UseConstant) Debug.LogError($"Trying to set float variable that is null!");
            } 
        }
    }

    [Serializable]
    [InlineProperty]
    public class FloatReference : Reference<float, FloatVariable> {}

    [Serializable]
    [InlineProperty]
    public class Vector2Reference : Reference<Vector2, Vector2Variable> {}

    [Serializable]
    [InlineProperty]
    public class Vector3Reference : Reference<Vector3, Vector3Variable> {}

    [Serializable]
    [InlineProperty]
    public class BoolReference : Reference<bool, BoolVariable> {}

    [Serializable]
    [InlineProperty]
    public class IntReference : Reference<int, IntVariable> {}

    [Serializable]
    [InlineProperty]
    public class QuaternionReference : Reference<Quaternion, QuaternionVariable> {}
    
    [Serializable]
    [InlineProperty]
    public class CurveReference : Reference<AnimationCurve, CurveVariable> {

        private float maxValue = float.MinValue;
        private float minValue = float.MaxValue;

        public CurveReference() : base() {
            if (Variable != null || UseConstant) {
                var curve = UseConstant ? ConstantValue : Variable.Value;
                foreach (var key in curve.keys)
                {
                    if (key.value < minValue) minValue = key.value;
                    if (key.value > maxValue) maxValue = key.value;
                }
            }
        }

        public float GetMaxValue() {
            return maxValue;
        }

        public float GetMinValue() {
            return minValue;
        }
        
        public float EvaluateFactor(float time)
        {
            var value = Value.Evaluate(time);
            var factor = (value - minValue) / (maxValue - minValue);
            return factor;
        }
    }
}