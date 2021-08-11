//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;

namespace JBooth.MicroSplat
{
#if __MICROSPLAT__
   [InitializeOnLoad]
   public class MicroSplatDecalModule : FeatureDescriptor
   {

      [MenuItem ("Window/MicroSplat/Create Decal")]
      static void AddEmitter ()
      {
         GameObject go = new GameObject ("Decal");
         go.transform.localScale = new Vector3 (10, 10, 10);
         go.AddComponent<MicroSplatDecal> ();
      }


      const string sDefine = "__MICROSPLAT_DECAL__";

      static MicroSplatDecalModule()
      {
         MicroSplatDefines.InitDefine(sDefine);
      }
      [PostProcessSceneAttribute (0)]
      public static void OnPostprocessScene()
      { 
         MicroSplatDefines.InitDefine(sDefine);
      }

      public override string ModuleName()
      {
         return "Decal";
      }

      public override string GetHelpPath ()
      {
         return "/MicroSplat/Core/Documentation/MicroSplat - Decals.pdf";
      }

      public enum DefineFeature
      {
         _DECAL,
         _DECAL_MAX0,
         _DECAL_MAX4,
         _DECAL_MAX8,
         _DECAL_MAX16,
         _DECAL_MAX32,
         _DECAL_MAX64,
         _DECAL_MAX128,
         _DECAL_MAX256,

         _DECAL_STATICMAX0,
         _DECAL_STATICMAX64,
         _DECAL_STATICMAX128,
         _DECAL_STATICMAX256,
         _DECAL_STATICMAX512,
         _DECAL_STATICMAX1024,
         _DECAL_STATICMAX2048,
         
         _DECAL_NOTEXTURES,
         _DECAL_EMISMETAL,
         _DECAL_SPLAT,
         _DECAL_TESS,
         _DECAL_TINT,
         kNumFeatures,
      }

      public enum MaxDecals
      {
         None,
         k4,
         k8,
         k16,
         k32,
         k64,
      }

      public enum MaxStaticDecals
      {
         None,
         k64,
         k128,
         k256,
         k512,
         k1024,
         k2048,
      }


      public bool decals = false;

      public MaxDecals maxDecals = MaxDecals.k16;
      public MaxStaticDecals maxStaticDecals = MaxStaticDecals.k512;

      public bool effectTextures = true;
      public bool effectSplats = false;
      public bool tint = false;
      public bool emisMetal = false;
#if __MICROSPLAT_TESSELLATION__
      public bool effectTess = false;
#endif
      static TextAsset properties;
      static TextAsset funcs;
      static TextAsset cbuffer;

      GUIContent CDecals = new GUIContent ("Decals", "Enable decal system");

      GUIContent CEffectTextures = new GUIContent ("Effect Diffuse/Normal", "Should decals modify the albedo and normal data");
      GUIContent CEffectEmisMetal = new GUIContent ("Support Emissive/Metallic map", "When enabled, you can use an emissive/metallic array with decals");
      GUIContent CEffectSplats = new GUIContent ("Effect Splat Maps", "Should decals modify the splat map or FX (wetness, puddles, streams, lava) data");
#if __MICROSPLAT_TESSELLATION__
      GUIContent CAffectTess = new GUIContent ("Effect Displacement", "Should decals modify the displacement when tessellation is enabled");
#endif

      GUIContent CMaxDecals = new GUIContent ("Max Dynamic", "Maximum number of dynamic decals allowed on a single object or terrain. Dynamic decals have a runtime GPU cost based on how many decals are on an object");
      GUIContent CMaxStaticDecals = new GUIContent ("Max Static", "Maximum number of static decals allowed on a single terrain. Static Decals have very small runtime cost, but are CPU intensive when moved or adjusted at runtime");

      GUIContent CAlbedo = new GUIContent ("Albedo Array", "Albedo/alpha for decals");
      GUIContent CEmisMetal = new GUIContent ("Emissive/Metal Array", "Emissive/Metallic array for decals");
      GUIContent CNormalSAO = new GUIContent ("Normal SAO Array", "NSAO Array for decals");
      GUIContent CSplat = new GUIContent ("Splat Map Array", "Array for splat maps, 3 texture weights (RGB) and alpha blend (A)");
      GUIContent CPerDecalTint = new GUIContent ("Decal Tint", "Allows you to tint each decal");

      static Dictionary<DefineFeature, string> sFeatureNames = new Dictionary<DefineFeature, string>();
      public static string GetFeatureName(DefineFeature feature)
      {
         string ret;
         if (sFeatureNames.TryGetValue(feature, out ret))
         {
            return ret;
         }
         string fn = System.Enum.GetName(typeof(DefineFeature), feature);
         sFeatureNames[feature] = fn;
         return fn;
      }

      public static bool HasFeature(string[] keywords, DefineFeature feature)
      {
         string f = GetFeatureName(feature);
         for (int i = 0; i < keywords.Length; ++i)
         {
            if (keywords[i] == f)
               return true;
         }
         return false;
      }


      public override string GetVersion()
      {
         return "3.8";
      }

      public override void DrawFeatureGUI(MicroSplatKeywords keywords)
      {
         decals = EditorGUILayout.Toggle (CDecals, decals);
         
         if (decals)
         {
            EditorGUI.indentLevel++;
            using (new GUILayout.VerticalScope (GUI.skin.box))
            {
               maxDecals = (MaxDecals)EditorGUILayout.EnumPopup (CMaxDecals, maxDecals);
               maxStaticDecals = (MaxStaticDecals)EditorGUILayout.EnumPopup (CMaxStaticDecals, maxStaticDecals);
            }
            effectTextures = EditorGUILayout.Toggle (CEffectTextures, effectTextures);
            emisMetal = EditorGUILayout.Toggle (CEffectEmisMetal, emisMetal);

            effectSplats = EditorGUILayout.Toggle (CEffectSplats, effectSplats);
#if __MICROSPLAT_TESSELLATION__
            effectTess = EditorGUILayout.Toggle (CAffectTess, effectTess);
#endif
            tint = EditorGUILayout.Toggle (CPerDecalTint, tint);
            EditorGUI.indentLevel--;
         }
      }

      public override void DrawShaderGUI(MicroSplatShaderGUI shaderGUI, MicroSplatKeywords keywords, Material mat, MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (decals)
         {
            if (MicroSplatUtilities.DrawRollup("Decal") && mat.HasProperty("_DecalAlbedo"))
            {
               if (effectTextures)
               {
                  var albedoMap = shaderGUI.FindProp ("_DecalAlbedo", props);
                  materialEditor.TexturePropertySingleLine (CAlbedo, albedoMap);
                  var normalMap = shaderGUI.FindProp ("_DecalNormalSAO", props);
                  materialEditor.TexturePropertySingleLine (CNormalSAO, normalMap);
               }
               if (emisMetal && mat.HasProperty("_DecalEmisMetal"))
               {
                  var emis = shaderGUI.FindProp ("_DecalEmisMetal", props);
                  materialEditor.TexturePropertySingleLine (CEmisMetal, emis);
               }
               if (effectSplats && mat.HasProperty("_DecalSplats"))
               {
                  var splatsMap = shaderGUI.FindProp ("_DecalSplats", props);
                  materialEditor.TexturePropertySingleLine (CSplat, splatsMap);
               }
            }
         }
      }

      public override string[] Pack()
      {
         List<string> features = new List<string>();
         if (decals)
         {
            features.Add(GetFeatureName(DefineFeature._DECAL));
            if (maxDecals == MaxDecals.None)
            {
               features.Add (GetFeatureName (DefineFeature._DECAL_MAX0));
            }
            else if (maxDecals == MaxDecals.k4)
            {
               features.Add (GetFeatureName (DefineFeature._DECAL_MAX4));
            }
            else if (maxDecals == MaxDecals.k8)
            {
               features.Add (GetFeatureName (DefineFeature._DECAL_MAX8));
            }
            else if (maxDecals == MaxDecals.k16)
            {
               features.Add (GetFeatureName (DefineFeature._DECAL_MAX16));
            }
            else if (maxDecals == MaxDecals.k32)
            {
               features.Add (GetFeatureName (DefineFeature._DECAL_MAX32));
            }
            else if (maxDecals == MaxDecals.k64)
            {
               features.Add (GetFeatureName (DefineFeature._DECAL_MAX64));
            }
            if (maxStaticDecals == MaxStaticDecals.None)
            {
               features.Add (GetFeatureName (DefineFeature._DECAL_STATICMAX0));
            }
            if (maxStaticDecals == MaxStaticDecals.k64)
            {
               features.Add (GetFeatureName (DefineFeature._DECAL_STATICMAX64));
            }
            else if (maxStaticDecals == MaxStaticDecals.k128)
            {
               features.Add (GetFeatureName (DefineFeature._DECAL_STATICMAX128));
            }
            else if (maxStaticDecals == MaxStaticDecals.k256)
            {
               features.Add (GetFeatureName (DefineFeature._DECAL_STATICMAX256));
            }
            else if (maxStaticDecals == MaxStaticDecals.k512)
            {
               features.Add (GetFeatureName (DefineFeature._DECAL_STATICMAX512));
            }
            else if (maxStaticDecals == MaxStaticDecals.k1024)
            {
               features.Add (GetFeatureName (DefineFeature._DECAL_STATICMAX1024));
            }
            else if (maxStaticDecals == MaxStaticDecals.k2048)
            {
               features.Add (GetFeatureName (DefineFeature._DECAL_STATICMAX2048));
            }

            if (!effectTextures)
            {
               features.Add (GetFeatureName (DefineFeature._DECAL_NOTEXTURES));
            }

            if (effectSplats)
            {
               features.Add (GetFeatureName (DefineFeature._DECAL_SPLAT));
            }
#if __MICROSPLAT_TESSELLATION__
            if (effectTess)
            {
               features.Add (GetFeatureName (DefineFeature._DECAL_TESS));
            }
#endif
            if (tint)
            {
               features.Add (GetFeatureName (DefineFeature._DECAL_TINT));
            }
            if (emisMetal)
            {
               features.Add (GetFeatureName (DefineFeature._DECAL_EMISMETAL));
            }
         }
         return features.ToArray();
      }

      public override void Unpack(string[] keywords)
      {
         decals = HasFeature (keywords, DefineFeature._DECAL);
         maxDecals = MaxDecals.k4;
         if (HasFeature(keywords, DefineFeature._DECAL_MAX0))
         {
            maxDecals = MaxDecals.None;
         }
         else if (HasFeature (keywords, DefineFeature._DECAL_MAX8))
         {
            maxDecals = MaxDecals.k8;
         }
         else if (HasFeature(keywords, DefineFeature._DECAL_MAX16))
         {
            maxDecals = MaxDecals.k16;
         }
         else if (HasFeature (keywords, DefineFeature._DECAL_MAX32))
         {
            maxDecals = MaxDecals.k32;
         }
         else if (HasFeature (keywords, DefineFeature._DECAL_MAX64))
         {
            maxDecals = MaxDecals.k64;
         }

         maxStaticDecals = MaxStaticDecals.k512;
         if (HasFeature(keywords, DefineFeature._DECAL_STATICMAX0))
         {
            maxStaticDecals = MaxStaticDecals.None;
         }
         else if (HasFeature (keywords, DefineFeature._DECAL_STATICMAX64))
         {
            maxStaticDecals = MaxStaticDecals.k64;
         }
         else if (HasFeature (keywords, DefineFeature._DECAL_STATICMAX128))
         {
            maxStaticDecals = MaxStaticDecals.k128;
         }
         else if (HasFeature (keywords, DefineFeature._DECAL_STATICMAX256))
         {
            maxStaticDecals = MaxStaticDecals.k256;
         }
         else if (HasFeature (keywords, DefineFeature._DECAL_STATICMAX512))
         {
            maxStaticDecals = MaxStaticDecals.k512;
         }
         else if (HasFeature (keywords, DefineFeature._DECAL_STATICMAX1024))
         {
            maxStaticDecals = MaxStaticDecals.k1024;
         }
         else if (HasFeature (keywords, DefineFeature._DECAL_STATICMAX2048))
         {
            maxStaticDecals = MaxStaticDecals.k2048;
         }

         effectTextures = !HasFeature (keywords, DefineFeature._DECAL_NOTEXTURES);
         emisMetal = HasFeature (keywords, DefineFeature._DECAL_EMISMETAL);
         effectSplats = HasFeature (keywords, DefineFeature._DECAL_SPLAT);
         tint = HasFeature (keywords, DefineFeature._DECAL_TINT);

#if __MICROSPLAT_TESSELLATION__
         effectTess = HasFeature (keywords, DefineFeature._DECAL_TESS);
#endif

      }

      public override void InitCompiler(string[] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths[i];
            if (p.EndsWith("microsplat_properties_decal.txt"))
            {
               properties = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_func_decal.txt"))
            {
               funcs = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith ("microsplat_cbuffer_decal.txt"))
            {
               cbuffer = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
         }
      }

      public override void WriteProperties(string[] features, System.Text.StringBuilder sb)
      {
         if (decals)
         {
            sb.Append(properties.text);
            if (emisMetal)
            {
               sb.AppendLine ("_DecalEmisMetal(\"Decal Emissive Metal\", 2DArray) = \"black\" {}");
            }
         }
      }

      public override void WriteFunctions(string [] features, System.Text.StringBuilder sb)
      {
         if (decals)
         {
            sb.Append(funcs.text);
         }
      }


      public override void WritePerMaterialCBuffer (string[] features, System.Text.StringBuilder sb)
      {
         if (decals)
         {
            sb.Append(cbuffer.text);
         }
      }

      public override void ComputeSampleCounts(string[] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {
         if (decals)
         {
            // this is rather arbitrary, could do some approximation based on static vs. dynamic counts, but wouldn't be any more or less
            // correct than this..
            arraySampleCount += 8; 
         }
#if __MICROSPLAT_TESSELLATION__
         if (effectTess)
         {
            tessellationSamples += 8;
         }
#endif

      }


   }   

#endif
}
