using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JBooth.MicroSplat;

[CustomEditor (typeof (MicroSplatDecalReceiver))]
public class MicroSplatDecalReceiverEditor : Editor
{
   public override void OnInspectorGUI ()
   {
      MicroSplatDecalReceiver dr = (MicroSplatDecalReceiver)target;
      if (dr.GetComponent<MicroSplatTerrain> () != null
#if __MICROSPLAT_MESHTERRAIN__
         || dr.GetComponent<MicroSplatMeshTerrain>() != null
#endif
         )
      {
         serializedObject.Update ();
         EditorGUILayout.PropertyField(serializedObject.FindProperty("generateCacheOnLoad"));
         EditorGUILayout.PropertyField(serializedObject.FindProperty("staticCacheSize"));

         serializedObject.ApplyModifiedProperties ();


         EditorGUILayout.Space ();
         EditorGUILayout.Space ();
         EditorGUILayout.LabelField ("Dynamic Count: " + dr.dynamicCount);

         EditorGUILayout.LabelField ("Static Count: " + dr.staticCount);


         if (dr.cacheMask != null)
         {
            Rect r = EditorGUILayout.GetControlRect (GUILayout.Width (256), GUILayout.Height (256));
            EditorGUI.DrawPreviewTexture (r, dr.cacheMask);
         }
      }
      else
      {
         EditorGUILayout.LabelField ("Dynamic Count: " + dr.dynamicCount);

      }
   }
}
