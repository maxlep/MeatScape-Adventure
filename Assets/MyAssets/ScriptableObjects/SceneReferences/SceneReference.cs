using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class SceneReference : ScriptableObject
{
    [TextArea (7, 10)] [HideInInlineEditors] public string Description;
}
