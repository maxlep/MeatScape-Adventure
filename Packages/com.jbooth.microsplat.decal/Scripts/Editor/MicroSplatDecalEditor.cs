using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using JBooth.MicroSplat;

[CustomEditor (typeof (MicroSplatDecal))]
public class MicroSplatDecalEditor : Editor
{
   static GUIContent CDynamic = new GUIContent ("Render Mode", "Static decals are fast to render, but slow to move. Dynamic decals are slow to render, but fast to move");
   static GUIContent CReceiver = new GUIContent ("Receiver", "Object which decal is applied to");

   static bool firstInit = true;

   enum DrawMode
   {
      Static,
      Dynamic
   }

   Bounds OnGetFrameBounds ()
   {
      MicroSplatDecal d = (MicroSplatDecal)target;
      Bounds bounds = new Bounds (d.transform.position, d.transform.lossyScale * 0.75f);
      return bounds;
   }

   bool HasFrameBounds ()
   {
      return true;
   }

   public override void OnInspectorGUI ()
   {
      if (firstInit)
      {
         firstInit = false;
         MicroSplatDecal.gizmoMode = (MicroSplatDecal.GizmoMode)EditorPrefs.GetInt ("MicroSplatDecal_GizmoMode", (int)MicroSplatDecal.gizmoMode);
         MicroSplatDecal.staticGizmoColor.r = EditorPrefs.GetFloat ("MicroSplatDecal_staticgizmocolor-r", MicroSplatDecal.staticGizmoColor.r);
         MicroSplatDecal.staticGizmoColor.g = EditorPrefs.GetFloat ("MicroSplatDecal_staticgizmocolor-g", MicroSplatDecal.staticGizmoColor.g);
         MicroSplatDecal.staticGizmoColor.b = EditorPrefs.GetFloat ("MicroSplatDecal_staticgizmocolor-b", MicroSplatDecal.staticGizmoColor.b);
         MicroSplatDecal.staticGizmoColor.a = EditorPrefs.GetFloat ("MicroSplatDecal_staticgizmocolor-a", MicroSplatDecal.staticGizmoColor.a);
         MicroSplatDecal.staticGizmoSelectedColor.r = EditorPrefs.GetFloat ("MicroSplatDecal_staticgizmoselectedcolor-r", MicroSplatDecal.staticGizmoSelectedColor.r);
         MicroSplatDecal.staticGizmoSelectedColor.g = EditorPrefs.GetFloat ("MicroSplatDecal_staticgizmoselectedcolor-g", MicroSplatDecal.staticGizmoSelectedColor.g);
         MicroSplatDecal.staticGizmoSelectedColor.b = EditorPrefs.GetFloat ("MicroSplatDecal_staticgizmoselectedcolor-b", MicroSplatDecal.staticGizmoSelectedColor.b);
         MicroSplatDecal.staticGizmoSelectedColor.a = EditorPrefs.GetFloat ("MicroSplatDecal_staticgizmoselectedcolor-a", MicroSplatDecal.staticGizmoSelectedColor.a);
         MicroSplatDecal.dynamicGizmoColor.r = EditorPrefs.GetFloat ("MicroSplatDecal_dynamicgizmocolor-r", MicroSplatDecal.dynamicGizmoColor.r);
         MicroSplatDecal.dynamicGizmoColor.g = EditorPrefs.GetFloat ("MicroSplatDecal_dynamicgizmocolor-g", MicroSplatDecal.dynamicGizmoColor.g);
         MicroSplatDecal.dynamicGizmoColor.b = EditorPrefs.GetFloat ("MicroSplatDecal_dynamicgizmocolor-b", MicroSplatDecal.dynamicGizmoColor.b);
         MicroSplatDecal.dynamicGizmoColor.a = EditorPrefs.GetFloat ("MicroSplatDecal_dynamicgizmocolor-a", MicroSplatDecal.dynamicGizmoColor.a);
         MicroSplatDecal.dynamicGizmoSelectedColor.r = EditorPrefs.GetFloat ("MicroSplatDecal_dynamicgizmoselectedcolor-r", MicroSplatDecal.dynamicGizmoSelectedColor.r);
         MicroSplatDecal.dynamicGizmoSelectedColor.g = EditorPrefs.GetFloat ("MicroSplatDecal_dynamicgizmoselectedcolor-g", MicroSplatDecal.dynamicGizmoSelectedColor.g);
         MicroSplatDecal.dynamicGizmoSelectedColor.b = EditorPrefs.GetFloat ("MicroSplatDecal_dynamicgizmoselectedcolor-b", MicroSplatDecal.dynamicGizmoSelectedColor.b);
         MicroSplatDecal.dynamicGizmoSelectedColor.a = EditorPrefs.GetFloat ("MicroSplatDecal_dynamicgizmocselectedolor-a", MicroSplatDecal.dynamicGizmoSelectedColor.a);
      }
      EditorGUI.BeginChangeCheck ();
      MicroSplatDecal d = (MicroSplatDecal)target;

      d.targetObject = EditorGUILayout.ObjectField(CReceiver, d.targetObject, typeof(MicroSplatDecalReceiver), true) as MicroSplatDecalReceiver;
      if (d.targetObject == null || d.targetObject.msObj == null)
      {
         return;
      }
      
      var msObj = d.targetObject.msObj;
      if (msObj.matInstance != null && msObj.matInstance.HasProperty ("_DecalAlbedo"))
      {
         Texture2DArray ta = null;
         if (!msObj.keywordSO.IsKeywordEnabled ("_DECAL_NOTEXTURES"))
         {
            ta = (Texture2DArray)msObj.matInstance.GetTexture ("_DecalAlbedo");
            if (ta != null)
            {
               d.textureIndex = JBooth.MicroSplat.MicroSplatUtilities.DrawTextureSelector (d.textureIndex, ta);
            }
         }
         using (new GUILayout.VerticalScope (GUI.skin.box))
         {
            d.dynamic = DrawMode.Dynamic == (DrawMode)EditorGUILayout.EnumPopup (CDynamic, d.dynamic == true ? DrawMode.Dynamic : DrawMode.Static);
         }

         if (!msObj.keywordSO.IsKeywordEnabled ("_DECAL_NOTEXTURES"))
         {
            using (new GUILayout.VerticalScope (GUI.skin.box))
            {
               d.albedoBlend = (MicroSplatDecal.AlbedoBlend)EditorGUILayout.EnumPopup ("Albedo Blend Mode", d.albedoBlend);
               d.albedoOpacity = EditorGUILayout.Slider ("Albedo Opacity", d.albedoOpacity, 0, 1);
               if (msObj.keywordSO.IsKeywordEnabled ("_DECAL_TINT"))
               {
                  d.tint = EditorGUILayout.ColorField ("Tint", d.tint);
               }
            }

            using (new GUILayout.VerticalScope (GUI.skin.box))
            {
               d.heightBlend = EditorGUILayout.Slider ("Height Blend", d.heightBlend, 0, 1);
            }
            using (new GUILayout.VerticalScope (GUI.skin.box))
            {
               d.normalBlend = (MicroSplatDecal.NormalBlend)EditorGUILayout.EnumPopup ("Normal Blend Mode", d.normalBlend);
               d.normalOpacity = EditorGUILayout.Slider ("Normal Opacity", d.normalOpacity, 0, 1);
            }

            using (new GUILayout.VerticalScope (GUI.skin.box))
            {
               d.smoothnessOpacity = EditorGUILayout.Slider ("Smoothness/AO Opacity", d.smoothnessOpacity, 0, 1);
            }
#if __MICROSPLAT_TESSELLATION__
            if (msObj.keywordSO.IsKeywordEnabled ("_DECAL_TESS") && msObj.keywordSO.IsKeywordEnabled ("_TESSDISTANCE"))
            {
               using (new GUILayout.VerticalScope (GUI.skin.box))
               {
                  d.tessOpacity = EditorGUILayout.Slider ("Displacement Opacity", d.tessOpacity, 0, 1);
                  d.tessOffset = EditorGUILayout.Slider ("Displacement Offset", d.tessOffset, -1, 1);
               }
            }
#endif
            
         }

         if (msObj.keywordSO.IsKeywordEnabled ("_DECAL_SPLAT"))
         {
            bool on = d.splatOpacity >= 0.01f;
            bool newOn = EditorGUILayout.Toggle ("Effect Splat Maps", on);
            if (newOn != on)
            {
               d.splatOpacity = newOn == false ? 0 : 0.5f;
            }
            if (newOn)
            { 
               Texture2DArray splatsArray = (Texture2DArray)msObj.templateMaterial.GetTexture ("_DecalSplats");
               if (splatsArray != null)
               {
                  d.splatTextureIndex = JBooth.MicroSplat.MicroSplatUtilities.DrawTextureSelector (d.splatTextureIndex, splatsArray);
                  d.splatOpacity = EditorGUILayout.Slider ("Splat Opacity", d.splatOpacity * 2, 0.01f, 2) * 0.5f;
                  d.splatMode = (MicroSplatDecal.SplatMode)EditorGUILayout.EnumPopup ("SplatMode", d.splatMode);

                  if (d.splatMode == MicroSplatDecal.SplatMode.SplatMap)
                  {
                     ta = msObj.templateMaterial.GetTexture ("_Diffuse") as Texture2DArray;
                     d.splatIndexes.x = JBooth.MicroSplat.MicroSplatUtilities.DrawTextureSelector ((int)d.splatIndexes.x, ta, true);
                     d.splatIndexes.y = JBooth.MicroSplat.MicroSplatUtilities.DrawTextureSelector ((int)d.splatIndexes.y, ta, true);
                     d.splatIndexes.z = JBooth.MicroSplat.MicroSplatUtilities.DrawTextureSelector ((int)d.splatIndexes.z, ta, true);
                     d.splatIndexes.w = JBooth.MicroSplat.MicroSplatUtilities.DrawTextureSelector ((int)d.splatIndexes.w, ta, true);
                  }
               }
               else
               {
                  EditorGUILayout.HelpBox ("Splat's array is enabled, but not assigned to the material. Please assign", MessageType.Error);
               }
            }
            
         }

      }
      using (new GUILayout.VerticalScope (GUI.skin.box))
      {
         d.sortOrder = EditorGUILayout.IntField ("Sort Order", d.sortOrder);
      }
      EditorGUILayout.Space ();
      MicroSplatTerrain mst = d.targetObject.msObj as MicroSplatTerrain;
      bool terrainExists = mst != null;

#if __MICROSPLAT_MESHTERRAIN__
      if ((d.targetObject.msObj as MicroSplatMeshTerrain) != null)
         terrainExists = true;
#endif

      if (terrainExists && GUILayout.Button ("Bake Static Decals"))
      {
         d.targetObject.RerenderCacheMap ();
      }

      EditorGUILayout.Space ();
      EditorGUILayout.Space ();

      if (JBooth.MicroSplat.MicroSplatUtilities.DrawRollup ("Display Settings", false, false))
      {
         EditorGUI.BeginChangeCheck ();
         MicroSplatDecal.gizmoMode = (MicroSplatDecal.GizmoMode)EditorGUILayout.EnumPopup ("Gizmo Mode", MicroSplatDecal.gizmoMode);
         if (MicroSplatDecal.gizmoMode != MicroSplatDecal.GizmoMode.Hide)
         {
            MicroSplatDecal.staticGizmoColor = EditorGUILayout.ColorField ("Static Gizmo Color", MicroSplatDecal.staticGizmoColor);
            MicroSplatDecal.staticGizmoSelectedColor = EditorGUILayout.ColorField ("Static Gizmo Selected Color", MicroSplatDecal.staticGizmoSelectedColor);
            MicroSplatDecal.dynamicGizmoColor = EditorGUILayout.ColorField ("Dynamic Gizmo Color", MicroSplatDecal.dynamicGizmoColor);
            MicroSplatDecal.dynamicGizmoSelectedColor = EditorGUILayout.ColorField ("Dynamic Gizmo Selected Color", MicroSplatDecal.dynamicGizmoSelectedColor);
         }
         if (EditorGUI.EndChangeCheck())
         {
            EditorPrefs.SetInt ("MicroSplatDecal_GizmoMode", (int)MicroSplatDecal.gizmoMode);
            EditorPrefs.SetFloat ("MicroSplatDecal_staticgizmocolor-r", MicroSplatDecal.staticGizmoColor.r);
            EditorPrefs.SetFloat ("MicroSplatDecal_staticgizmocolor-g", MicroSplatDecal.staticGizmoColor.g);
            EditorPrefs.SetFloat ("MicroSplatDecal_staticgizmocolor-b", MicroSplatDecal.staticGizmoColor.b);
            EditorPrefs.SetFloat ("MicroSplatDecal_staticgizmocolor-a", MicroSplatDecal.staticGizmoColor.a);
            EditorPrefs.SetFloat ("MicroSplatDecal_staticgizmoselectedcolor-r", MicroSplatDecal.staticGizmoSelectedColor.r);
            EditorPrefs.SetFloat ("MicroSplatDecal_staticgizmoselectedcolor-g", MicroSplatDecal.staticGizmoSelectedColor.g);
            EditorPrefs.SetFloat ("MicroSplatDecal_staticgizmoselectedcolor-b", MicroSplatDecal.staticGizmoSelectedColor.b);
            EditorPrefs.SetFloat ("MicroSplatDecal_staticgizmoselectedcolor-a", MicroSplatDecal.staticGizmoSelectedColor.a);
            EditorPrefs.SetFloat ("MicroSplatDecal_dynamicgizmocolor-r", MicroSplatDecal.dynamicGizmoColor.r);
            EditorPrefs.SetFloat ("MicroSplatDecal_dynamicgizmocolor-g", MicroSplatDecal.dynamicGizmoColor.g);
            EditorPrefs.SetFloat ("MicroSplatDecal_dynamicgizmocolor-b", MicroSplatDecal.dynamicGizmoColor.b);
            EditorPrefs.SetFloat ("MicroSplatDecal_dynamicgizmocolor-a", MicroSplatDecal.dynamicGizmoColor.a);
            EditorPrefs.SetFloat ("MicroSplatDecal_dynamicgizmoselectedcolor-r", MicroSplatDecal.dynamicGizmoSelectedColor.r);
            EditorPrefs.SetFloat ("MicroSplatDecal_dynamicgizmoselectedcolor-g", MicroSplatDecal.dynamicGizmoSelectedColor.g);
            EditorPrefs.SetFloat ("MicroSplatDecal_dynamicgizmoselectedcolor-b", MicroSplatDecal.dynamicGizmoSelectedColor.b);
            EditorPrefs.SetFloat ("MicroSplatDecal_dynamicgizmocselectedolor-a", MicroSplatDecal.dynamicGizmoSelectedColor.a);
         }
      }
      if (EditorGUI.EndChangeCheck())
      {
         d.targetObject.RerenderCacheMap ();
         EditorUtility.SetDirty (d);
         d.Reset ();
      }
      MicroSplatDecalReceiver dr = d.targetObject;
      if (dr.cacheMask != null && terrainExists) 
      {
         Rect r = EditorGUILayout.GetControlRect (GUILayout.Width (256), GUILayout.Height (256));
         EditorGUI.DrawPreviewTexture (r, dr.cacheMask);
      }

   }
}
