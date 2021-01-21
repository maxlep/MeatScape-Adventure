using Sirenix.OdinInspector;
using UnityEngine;

public class SceneReference<T> : ScriptableObject
{
    [TextArea (7, 10)] [HideInInlineEditors] public string Description;
    [SerializeField] private T reference;
    
    public T Value
    {
        get => reference;
        set => reference = value;
    }
}
