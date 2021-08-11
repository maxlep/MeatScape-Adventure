//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using JBooth.MicroSplat;

#if __MICROSPLAT__
public class MicroSplatShaderObjectGUI : ShaderGUI
{
   MicroSplatShaderGUI.MicroSplatCompiler compiler = new MicroSplatShaderGUI.MicroSplatCompiler ();

   static GUIContent CAlphaMode = new GUIContent ("Mode", "Rendering Mode");

   static GUIContent CStandardDiffuse = new GUIContent ("Albedo", "Albedo texture");
   static GUIContent CStandardNormal = new GUIContent ("Normal", "Normal Map");
   static GUIContent CStandardNormalStrength = new GUIContent ("Normal Strength", "Strength of normal map");
   static GUIContent CStandardDetailPacked = new GUIContent ("Detail Packed", "Detail with diffuse in R, normal in G/A, and smoothness in B");
   static GUIContent CStandardEmission = new GUIContent ("Emission", "Emissive texture");
   static GUIContent CStandardSpecular = new GUIContent ("Specular", "Specular Texture");
   static GUIContent CStandardPackedMap = new GUIContent ("Mask Map", "(R) Metallic, (G) Occlusion, (B) Detail Mask (A) Smoothness");
   static GUIContent CStandardDetailUVScale = new GUIContent ("Detail UV Scale/Offset", "Scale and offset for UVs on the detail textures");
   static GUIContent CStandardMetallic = new GUIContent ("Metallic", "Metallic value");
   static GUIContent CStandardSmoothness = new GUIContent ("Smoothness", "Smoothness value");
   static GUIContent CStandardDetailAlbedoStrength = new GUIContent ("Detail Albedo Strength", "Strength of detail albedo");
   static GUIContent CStandardDetailSmoothnessStrength = new GUIContent ("Detail Smoothness Strength", "Strength of detail smoothness");
   static GUIContent CStandardDetailNormalScale = new GUIContent ("Detail Normal Strength", "Strength of detail normal");
   static GUIContent CAlphaThreshold = new GUIContent ("Cutout Threshold", "Alpha values below this level get clipped");
   public enum AlphaMode
   {
      Opaque,
      Cutout,
   }

   bool oldEnabled = false;
   bool BeginDrawKeywordToggle(Material mat, string keyword)
   {
      oldEnabled = GUI.enabled;
      bool on = mat.IsKeywordEnabled (keyword);

      var newOn = EditorGUILayout.Toggle (on, GUILayout.Width (22));
      if (newOn != on)
      {
         if (newOn)
            mat.EnableKeyword (keyword);
         else
            mat.DisableKeyword (keyword);
      }
      GUI.enabled = newOn;
      return newOn;
   }

   void EndDrawKeywordToggle()
   {
      GUI.enabled = oldEnabled;
   }

   public override void OnGUI (MaterialEditor materialEditor, MaterialProperty [] props)
   {

      var targetMat = materialEditor.target as Material;

      compiler.Init ();
      
      var path = AssetDatabase.GetAssetPath (targetMat.shader);
      path = path.Replace ("_objects.shader", ".mat");
      path = path.Replace("_objects.surfshader", ".mat");
      
      var sourceMat = AssetDatabase.LoadAssetAtPath<Material> (path);
      if (sourceMat == null)
      {
         EditorGUILayout.HelpBox ("Source Material not found at path : " + path + "\nMost like you renamed either the material or shader in your MicroSplatData directory", MessageType.Error);
         return;
      }
      var keywords = MicroSplatUtilities.FindOrCreateKeywords (sourceMat);

      MicroSplatKeywords fakeKeywords = MicroSplatKeywords.CreateInstance<MicroSplatKeywords> ();
      fakeKeywords.keywords = new List<string> (keywords.keywords);
      MicroSplatObjectShaderModule.ModifyKeywords (fakeKeywords.keywords);
      MicroSplatShaderGUI shaderGUI = new MicroSplatShaderGUI ();


      // gui for the regular shader part


      if (MicroSplatUtilities.DrawRollup ("Standard Material"))
      {
         using (new GUILayout.VerticalScope (GUI.skin.box))
         {
            AlphaMode mode = AlphaMode.Opaque;
            if (targetMat.IsKeywordEnabled ("_OBJECTSHADERALPHACLIP"))
               mode = AlphaMode.Cutout;

            var newMode = (AlphaMode)EditorGUILayout.EnumPopup (CAlphaMode, mode);
            if (newMode != mode)
            {
               if (newMode == AlphaMode.Cutout)
               {
                  targetMat.EnableKeyword ("_OBJECTSHADERALPHACLIP");
               }
               else
               {
                  targetMat.DisableKeyword ("_OBJECTSHADERALPHACLIP");
               }
            }



            if (targetMat.HasProperty ("_ObjectShaderDiffuse"))
            {
               EditorGUILayout.BeginHorizontal ();
               materialEditor.TexturePropertySingleLine (CStandardDiffuse, shaderGUI.FindProp ("_ObjectShaderDiffuse", props));
               var colorProp = shaderGUI.FindProp ("_ObjectShaderDiffuseTint", props);
               var c = colorProp.colorValue;
               var nc = EditorGUILayout.ColorField (c, GUILayout.Width (60));
               if (c != nc)
               {
                  colorProp.colorValue = nc;
               }
               EditorGUILayout.EndHorizontal ();

               if (newMode == AlphaMode.Cutout)
               {
                  materialEditor.RangeProperty (shaderGUI.FindProp ("_ObjectShaderAlphaClipThreshold", props), CAlphaThreshold.text);
               }

               materialEditor.TexturePropertySingleLine (CStandardNormal, shaderGUI.FindProp ("_ObjectShaderNormal", props));
               materialEditor.RangeProperty (shaderGUI.FindProp ("_ObjectShaderNormalScale", props), CStandardNormalStrength.text);

               EditorGUILayout.BeginHorizontal ();
               bool enabled = BeginDrawKeywordToggle (targetMat, "_OBJECTSHADERPACKEDMAP");
               MicroSplatUtilities.WarnLinear (shaderGUI.FindProp ("_ObjectShaderPackedMap", props).textureValue as Texture2D);
               materialEditor.TexturePropertySingleLine (CStandardPackedMap, shaderGUI.FindProp ("_ObjectShaderPackedMap", props));
               EditorGUILayout.EndHorizontal ();
               EndDrawKeywordToggle ();
               if (!enabled)
               {
                  materialEditor.RangeProperty (shaderGUI.FindProp ("_ObjectShaderMetallic", props), CStandardMetallic.text);
                  materialEditor.RangeProperty (shaderGUI.FindProp ("_ObjectShaderSmoothness", props), CStandardSmoothness.text);
               }
            }

            EditorGUILayout.BeginHorizontal ();
            BeginDrawKeywordToggle (targetMat, "_OBJECTSHADEREMISSION");
            materialEditor.TexturePropertySingleLine (CStandardEmission, shaderGUI.FindProp ("_ObjectShaderEmission", props));
            EditorGUILayout.EndHorizontal ();
            EndDrawKeywordToggle ();

            materialEditor.ShaderProperty (shaderGUI.FindProp ("_ObjectShaderUVScaleOffset", props), "UV Scale/Offset");

            if (keywords.IsKeywordEnabled ("_USESPECULARWORKFLOW"))
            {
               EditorGUILayout.BeginHorizontal ();
               BeginDrawKeywordToggle (targetMat, "_OBJECTSHADERSPECULAR");
               materialEditor.TexturePropertySingleLine (CStandardSpecular, shaderGUI.FindProp ("_ObjectShaderSpecular", props));
               EditorGUILayout.EndHorizontal ();
               EndDrawKeywordToggle ();
            }
         }


         using (new GUILayout.VerticalScope (GUI.skin.box))
         {
            EditorGUILayout.BeginHorizontal ();
            BeginDrawKeywordToggle (targetMat, "_OBJECTSHADERDETAILPACKED");
            MicroSplatUtilities.WarnLinear (shaderGUI.FindProp ("_ObjectShaderDetailNormal", props).textureValue as Texture2D);
            materialEditor.TexturePropertySingleLine (CStandardDetailPacked, shaderGUI.FindProp ("_ObjectShaderDetailNormal", props));
            EditorGUILayout.EndHorizontal ();
            
            materialEditor.ShaderProperty (shaderGUI.FindProp ("_ObjectShaderDetailUVScaleOffset", props), CStandardDetailUVScale);
            materialEditor.RangeProperty (shaderGUI.FindProp ("_ObjectShaderDetailAlbedoStrength", props), CStandardDetailAlbedoStrength.text);
            materialEditor.RangeProperty (shaderGUI.FindProp ("_ObjectShaderDetailNormalScale", props), CStandardDetailNormalScale.text);
            materialEditor.RangeProperty (shaderGUI.FindProp ("_ObjectShaderDetailSmoothnessStrength", props), CStandardDetailSmoothnessStrength.text);
            EndDrawKeywordToggle ();
         }
      }



      if (GUILayout.Button ("Sync from Source Mat") && sourceMat.shader != null)
      {
         // because the build in copy properties is not addative, but wipes existing data..
         int count = ShaderUtil.GetPropertyCount (sourceMat.shader);
         for (int i = 0; i < count; ++i)
         {
            var t = ShaderUtil.GetPropertyType (sourceMat.shader, i);
            var name = ShaderUtil.GetPropertyName (sourceMat.shader, i);
            switch (t)
            {
            case ShaderUtil.ShaderPropertyType.Range:
            case ShaderUtil.ShaderPropertyType.Float:
               if (targetMat.HasProperty (name)) { targetMat.SetFloat (name, sourceMat.GetFloat (name)); }
               break;
            case ShaderUtil.ShaderPropertyType.Color:
               if (targetMat.HasProperty (name)) { targetMat.SetColor (name, sourceMat.GetColor (name)); }
               break;
            case ShaderUtil.ShaderPropertyType.Vector:
               if (targetMat.HasProperty (name)) { targetMat.SetVector (name, sourceMat.GetVector (name)); }
               break;
            case ShaderUtil.ShaderPropertyType.TexEnv:
               if (targetMat.HasProperty (name)) { targetMat.SetTexture (name, sourceMat.GetTexture (name)); }
               break;
            }
         }
      }

      // gui for the rest..

      foreach (var e in compiler.extensions)
      {
         e.Unpack (fakeKeywords.keywords.ToArray());
      }


      using (new GUILayout.VerticalScope (MicroSplatUtilities.boxStyle))
      {
         foreach (var e in compiler.extensions)
         {
            e.DrawShaderGUI (shaderGUI, fakeKeywords, targetMat, materialEditor, props);
         }
      }
      Object.DestroyImmediate (fakeKeywords);


   }

}
#endif
