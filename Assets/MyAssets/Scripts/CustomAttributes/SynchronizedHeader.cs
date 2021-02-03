

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using Sirenix.OdinInspector.Editor;
using UnityEditor;

public class SynchronizedHeaderAttributeDrawer : OdinAttributeDrawer<SynchronizedHeader>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        var rect = EditorGUILayout.GetControlRect();
        EditorGUI.LabelField(rect, Property.NiceName, EditorStyles.boldLabel);
        CallNextDrawer(null);
    }
}

#endif

public class SynchronizedHeader : Attribute
{
    
}
