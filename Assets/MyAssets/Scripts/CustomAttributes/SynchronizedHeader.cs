using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

public class SynchronizedHeader : Attribute
{
    
}

#if UNITY_EDITOR

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
