using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Required]
[CreateAssetMenu(fileName = "TriggerVariable", menuName = "Variables/TriggerVariable", order = 0)]
public class TriggerVariable : ScriptableObject
{
    [TextArea] [HideInInlineEditors] public String Description;
    
    public delegate void OnUpdate_();
    public event OnUpdate_ OnUpdate;

    public void Activate()
    {
        OnUpdate?.Invoke();
    }
}
