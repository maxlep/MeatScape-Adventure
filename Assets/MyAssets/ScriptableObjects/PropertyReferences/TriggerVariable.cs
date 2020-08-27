using System;
using Sirenix.OdinInspector;
using UnityEngine;

[InlineEditor(InlineEditorObjectFieldModes.Foldout)]
[CreateAssetMenu(fileName = "TriggerVariable", menuName = "Variables/TriggerVariable", order = 0)]
public class TriggerVariable : ScriptableObject
{
    [TextArea] [HideInInlineEditors] public String Description;
    
    public delegate void OnUpdate_();
    public event OnUpdate_ OnUpdate;

    public void Activate()
    {
        Debug.Log("ACTIVATED");
        OnUpdate?.Invoke();
    }
}
