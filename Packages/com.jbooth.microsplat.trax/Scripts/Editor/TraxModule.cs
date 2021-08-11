//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;
using System.Text;

namespace JBooth.MicroSplat
{
#if __MICROSPLAT__
   [InitializeOnLoad]
   public class MicroSplatTrax : FeatureDescriptor
   {
      const string sDefine = "__MICROSPLAT_TRAX__";
      static MicroSplatTrax ()
      {
         MicroSplatDefines.InitDefine (sDefine);
      }
      [PostProcessSceneAttribute (0)]
      public static void OnPostprocessScene ()
      {
         MicroSplatDefines.InitDefine (sDefine);
      }

      public override string ModuleName ()
      {
         return "Trax";
      }

      public override string GetHelpPath ()
      {
         return "/MicroSplat/Core/Documentation/MicroSplat - Trax.pdf";
      }

      public enum DefineFeature
      {
         _SNOWFOOTSTEPS,
         _TRAXSINGLE,
         _TRAXARRAY,
         _TRAXNOTEXTURE,
         _PERTEXTRAXDIGDEPTH,
         _PERTEXTRAXOPACITY,
         _PERTEXTRAXNORMALSTR,
         _PERTEXTRAXTINT,
         _TRAXQUADRATIC,
         kNumFeatures,
      }

      public bool snowFootsteps = false;
      public enum SampleMode
      {
         Bilinear,
         Quadratic
      }

      public enum TraxTextureMode
      {
         None,
         NoTexture,
         Single,
         Array,
         
      }

      public TraxTextureMode texMode = TraxTextureMode.None;
      public bool perTexDigDepth;
      public bool perTexOpacity;
      public bool perTexNormalStr;
      public bool perTexTraxTint;
      public SampleMode sampleMode = SampleMode.Bilinear;

      TextAsset cbuffer;
      TextAsset func;


      GUIContent CSnowFootsteps = new GUIContent("Snow Tracks", "When enabled allows footsteps and other deformations in snow");
      GUIContent CTexMode = new GUIContent ("Trax Texture", "Allows you to have a single trax texture for all terrain types, or you can use a texture array to provide one for each terrain type, or you can skip the texture and just use tint and such");
      GUIContent CSampleMode = new GUIContent ("Sample Mode", "Quadratic provides a higher quality sampling of the trax buffer, smoothing the edges");


      // Can we template these somehow?
      static Dictionary<DefineFeature, string> sFeatureNames = new Dictionary<DefineFeature, string> ();
      public static string GetFeatureName (DefineFeature feature)
      {
         string ret;
         if (sFeatureNames.TryGetValue (feature, out ret))
         {
            return ret;
         }
         string fn = System.Enum.GetName (typeof (DefineFeature), feature);
         sFeatureNames [feature] = fn;
         return fn;
      }

      public static bool HasFeature (string [] keywords, DefineFeature feature)
      {
         string f = GetFeatureName (feature);
         for (int i = 0; i < keywords.Length; ++i)
         {
            if (keywords [i] == f)
               return true;
         }
         return false;
      }

      public static bool HasFeature (string [] keywords, string f)
      {
         for (int i = 0; i < keywords.Length; ++i)
         {
            if (keywords [i] == f)
               return true;
         }
         return false;
      }


      public override string GetVersion ()
      {
         return "3.8";
      }

      public override void DrawFeatureGUI (MicroSplatKeywords keywords)
      {
         snowFootsteps = EditorGUILayout.Toggle(CSnowFootsteps, snowFootsteps);
         texMode = (TraxTextureMode)EditorGUILayout.EnumPopup (CTexMode, texMode);
         if (snowFootsteps || texMode != TraxTextureMode.None)
         {
            EditorGUI.indentLevel++;
            sampleMode = (SampleMode)EditorGUILayout.EnumPopup (CSampleMode, sampleMode);
            EditorGUI.indentLevel--;
         }
      }

      static GUIContent CFootstepDiffTex = new GUIContent("Diffuse/Height", "Diffuse with height map in alpha for snow");
      static GUIContent CFootstepNormTex = new GUIContent("NormalSAO", "Normal, smoothness, and ao for snow");
      static GUIContent CErosionClearing = new GUIContent ("Erosion Clearing", "Causes snow to clear on cliff edges and based on AO");
      static GUIContent CHeightClearing = new GUIContent ("Height Clearing", "Causes snow to clear on the tops of the underlying terrain");
      static GUIContent CCrystals = new GUIContent ("Crystals", "Blend between soft and icy snow");
      static GUIContent CWetnessLevel = new GUIContent ("Wetness", "Wetness level for fully depressed areas");
      static GUIContent CPuddleLevel = new GUIContent ("Puddles", "Puddles level for fully depressed areas");
      static GUIContent CStreamLevel = new GUIContent ("Streams", "Streams level for fully depressed areas");
      static GUIContent CLavaLevel = new GUIContent ("Lava", "Lava level for fully depressed areas");

      public override void DrawShaderGUI (MicroSplatShaderGUI shaderGUI, MicroSplatKeywords keywords, Material mat, MaterialEditor materialEditor, MaterialProperty [] props)
      {
         bool snow = keywords.IsKeywordEnabled ("_SNOW");
         
         if ((texMode != TraxTextureMode.None || (snow && snowFootsteps)) && MicroSplatUtilities.DrawRollup ("Trax"))
         {
            if (texMode == TraxTextureMode.Single || texMode == TraxTextureMode.Array)
            {
               if (mat.HasProperty("_TraxInterpContrast"))
               {
                  materialEditor.ShaderProperty (shaderGUI.FindProp("_TraxInterpContrast", props), "Interpolation Contrast");
               }
            }
            if (mat.HasProperty ("_TraxFXThresholds"))
            {
               Vector4 v = mat.GetVector ("_TraxFXThresholds");
               EditorGUI.BeginChangeCheck ();

               if (keywords.IsKeywordEnabled("_WETNESS"))
               {
                  v.x = EditorGUILayout.Slider (CWetnessLevel, v.x, 0, 1);
               }
               if (keywords.IsKeywordEnabled("_PUDDLES"))
               {
                  v.y = EditorGUILayout.Slider (CPuddleLevel, v.y, 0, 1);
               }
               if (keywords.IsKeywordEnabled ("_STREAMS"))
               {
                  v.z = EditorGUILayout.Slider (CStreamLevel, v.z, 0, 1);
               }
               if (keywords.IsKeywordEnabled ("_LAVA"))
               {
                  v.w = EditorGUILayout.Slider (CLavaLevel, v.w, 0, 1);
               }

               if (EditorGUI.EndChangeCheck())
               {
                  mat.SetVector ("_TraxFXThresholds", v);
                  EditorUtility.SetDirty (mat);
               }
               
            }
            if (texMode == TraxTextureMode.NoTexture && MicroSplatUtilities.DrawRollup("No Texture", true, true))
            {
               if (mat.HasProperty ("_TraxNormalStrength"))
               {
                  materialEditor.RangeProperty (shaderGUI.FindProp ("_TraxNormalStrength", props), "Normal Strength");
               }
            }
            if (texMode == TraxTextureMode.Single && MicroSplatUtilities.DrawRollup ("Single Texture", true, true))
            {
               if (mat.HasProperty ("_TraxDiff"))
               {
                  var trackDiff = shaderGUI.FindProp ("_TraxDiff", props);
                  var trackNorm = shaderGUI.FindProp ("_TraxNSAO", props);
                  materialEditor.TexturePropertySingleLine (CFootstepDiffTex, trackDiff);
                  materialEditor.TexturePropertySingleLine (CFootstepNormTex, trackNorm);
                  MicroSplatUtilities.EnforceDefaultTexture (trackDiff, "microsplat_def_trax_mud_diff");
                  MicroSplatUtilities.EnforceDefaultTexture (trackNorm, "microsplat_def_trax_mud_normSAO");
                  Vector4 uvs = shaderGUI.FindProp ("_TraxUVScales", props).vectorValue;
                  EditorGUI.BeginChangeCheck ();
                  EditorGUILayout.BeginHorizontal ();
                  EditorGUILayout.PrefixLabel ("UV Scale");
                  uvs.x = EditorGUILayout.FloatField (uvs.x);
                  uvs.y = EditorGUILayout.FloatField (uvs.y);
                  EditorGUILayout.EndHorizontal ();
                  if (EditorGUI.EndChangeCheck ())
                  {
                     shaderGUI.FindProp ("_TraxUVScales", props).vectorValue = uvs;
                     EditorUtility.SetDirty (mat);
                  }
               }

               if (mat.HasProperty ("_TraxTextureBlend"))
               {
                  materialEditor.RangeProperty (shaderGUI.FindProp ("_TraxTextureBlend", props), "Texture Blend");
               }
               if (mat.HasProperty ("_TraxNormalStrength"))
               {
                  materialEditor.RangeProperty (shaderGUI.FindProp ("_TraxNormalStrength", props), "Normal Strength");
               }
            }

            if (texMode == TraxTextureMode.Array && MicroSplatUtilities.DrawRollup ("Array", true, true))
            {
               if (mat.HasProperty ("_TraxArrayDiff"))
               {
                  var trackDiff = shaderGUI.FindProp ("_TraxArrayDiff", props);
                  var trackNorm = shaderGUI.FindProp ("_TraxArrayNSAO", props);
                  materialEditor.TexturePropertySingleLine (CFootstepDiffTex, trackDiff);
                  materialEditor.TexturePropertySingleLine (CFootstepNormTex, trackNorm);
                  Vector4 uvs = shaderGUI.FindProp ("_TraxUVScales", props).vectorValue;
                  EditorGUI.BeginChangeCheck ();
                  EditorGUILayout.BeginHorizontal ();
                  EditorGUILayout.PrefixLabel ("UV Scale");
                  uvs.x = EditorGUILayout.FloatField (uvs.x);
                  uvs.y = EditorGUILayout.FloatField (uvs.y);
                  EditorGUILayout.EndHorizontal ();
                  if (EditorGUI.EndChangeCheck ())
                  {
                     shaderGUI.FindProp ("_TraxUVScales", props).vectorValue = uvs;
                     EditorUtility.SetDirty (mat);
                  }
               }

               if (mat.HasProperty ("_TraxTextureBlend"))
               {
                  materialEditor.RangeProperty (shaderGUI.FindProp ("_TraxTextureBlend", props), "Texture Blend");
               }
               if (mat.HasProperty ("_TraxNormalStrength"))
               {
                  materialEditor.RangeProperty (shaderGUI.FindProp ("_TraxNormalStrength", props), "Normal Strength");
               }
            }


            if (snow && MicroSplatUtilities.DrawRollup("Snow Trax", true, true))
            {
               // snow specific..
               if (snowFootsteps && mat.HasProperty ("_SnowTrackDiff"))
               {
                  var trackDiff = shaderGUI.FindProp ("_SnowTrackDiff", props);
                  var trackNorm = shaderGUI.FindProp ("_SnowTrackNSAO", props);
                  materialEditor.TexturePropertySingleLine (CFootstepDiffTex, trackDiff);
                  materialEditor.TexturePropertySingleLine (CFootstepNormTex, trackNorm);
                  MicroSplatUtilities.EnforceDefaultTexture (trackDiff, "microsplat_def_trax_snow_diff");
                  MicroSplatUtilities.EnforceDefaultTexture (trackNorm, "microsplat_def_trax_snow_nsao");
                  Vector4 snowUV = shaderGUI.FindProp ("_SnowTraxUVScales", props).vectorValue;
                  EditorGUI.BeginChangeCheck ();
                  EditorGUILayout.BeginHorizontal ();
                  EditorGUILayout.PrefixLabel ("UV Scale");
                  snowUV.x = EditorGUILayout.FloatField (snowUV.x);
                  snowUV.y = EditorGUILayout.FloatField (snowUV.y);
                  EditorGUILayout.EndHorizontal ();
                  if (EditorGUI.EndChangeCheck ())
                  {
                     shaderGUI.FindProp ("_SnowTraxUVScales", props).vectorValue = snowUV;
                     EditorUtility.SetDirty (mat);
                  }
               }
               if (mat.HasProperty("_TraxSnowTint"))
               {
                  materialEditor.ColorProperty(shaderGUI.FindProp("_TraxSnowTint", props), "Tint");
               }

               if (mat.HasProperty ("_SnowTraxTextureBlend"))
               {
                  materialEditor.RangeProperty (shaderGUI.FindProp ("_SnowTraxTextureBlend", props), "Texture Blend");
               }
               if (mat.HasProperty ("_SnowTraxNormalStrength"))
               {
                  materialEditor.RangeProperty (shaderGUI.FindProp ("_SnowTraxNormalStrength", props), "Normal Strength");
               }

               if (mat.HasProperty("_TraxSnowErosion"))
               {
                  materialEditor.RangeProperty (shaderGUI.FindProp ("_TraxSnowErosion", props), CErosionClearing.text);
               }
               if (mat.HasProperty("_TraxSnowHeight"))
               {
                  materialEditor.RangeProperty (shaderGUI.FindProp ("_TraxSnowHeight", props), CHeightClearing.text);
               }
               if (mat.HasProperty ("_TraxSnowAge"))
               {
                  materialEditor.FloatProperty (shaderGUI.FindProp ("_TraxSnowAge", props), CCrystals.text);
               }
               if (mat.HasProperty("_TraxSnowRemoval"))
               {
                  materialEditor.RangeProperty (shaderGUI.FindProp ("_TraxSnowRemoval", props), "Snow Removal");
               }
            }
         }
      }

      public override string [] Pack ()
      {
         List<string> features = new List<string> ();
         if (snowFootsteps)
         {
            features.Add (GetFeatureName (DefineFeature._SNOWFOOTSTEPS));
         }
         if (texMode == TraxTextureMode.Single)
         {
            features.Add (GetFeatureName (DefineFeature._TRAXSINGLE));
         }
         else if (texMode == TraxTextureMode.Array)
         {
            features.Add (GetFeatureName (DefineFeature._TRAXARRAY));
         }
         else if (texMode == TraxTextureMode.NoTexture)
         {
            features.Add (GetFeatureName (DefineFeature._TRAXNOTEXTURE));
         }
         if (perTexOpacity)
         {
            features.Add (GetFeatureName (DefineFeature._PERTEXTRAXOPACITY));
         }
         if (perTexNormalStr)
         {
            features.Add (GetFeatureName (DefineFeature._PERTEXTRAXNORMALSTR));
         }
         if (perTexDigDepth)
         {
            features.Add (GetFeatureName (DefineFeature._PERTEXTRAXDIGDEPTH));
         }
         if (perTexTraxTint)
         {
            features.Add (GetFeatureName (DefineFeature._PERTEXTRAXTINT));
         }
         if (sampleMode == SampleMode.Quadratic)
         {
            features.Add (GetFeatureName (DefineFeature._TRAXQUADRATIC));
         }
         return features.ToArray ();
      }

      public override void Unpack (string [] keywords)
      {
         snowFootsteps = HasFeature (keywords, DefineFeature._SNOWFOOTSTEPS);
         texMode = TraxTextureMode.None;
         if (HasFeature(keywords, DefineFeature._TRAXSINGLE))
         {
            texMode = TraxTextureMode.Single;
         }
         else if (HasFeature (keywords, DefineFeature._TRAXARRAY))
         {
            texMode = TraxTextureMode.Array;
         }
         else if (HasFeature(keywords, DefineFeature._TRAXNOTEXTURE))
         {
            texMode = TraxTextureMode.NoTexture;
         }
         perTexOpacity = HasFeature (keywords, DefineFeature._PERTEXTRAXOPACITY);
         perTexNormalStr = HasFeature (keywords, DefineFeature._PERTEXTRAXNORMALSTR);
         perTexDigDepth = HasFeature (keywords, DefineFeature._PERTEXTRAXDIGDEPTH);
         perTexTraxTint = HasFeature (keywords, DefineFeature._PERTEXTRAXTINT);
         sampleMode = HasFeature (keywords, DefineFeature._TRAXQUADRATIC) ? SampleMode.Quadratic : SampleMode.Bilinear;
      }

      public override void InitCompiler (string [] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths [i];
            if (p.EndsWith ("microsplat_func_trax.txt"))
            {
               func = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
            if (p.EndsWith ("microsplat_cbuffer_trax.txt"))
            {
               cbuffer = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
         }
      }

      public override void WriteProperties (string [] features, System.Text.StringBuilder sb)
      {

         if (snowFootsteps && HasFeature (features, "_SNOW"))
         {
            
            sb.AppendLine ("      _SnowTrackDiff(\"Track Diffuse\", 2D) = \"white\" {}");
            sb.AppendLine ("      _TraxSnowTint(\"Snow Tint\", Color) = (1,1,1,1)");
            sb.AppendLine ("      _SnowTrackNSAO(\"Track NSAO\", 2D) = \"white\" {}");
            sb.AppendLine ("      _SnowTraxUVScales(\"UV scale\", Vector) = (1,1,0,0)");
            sb.AppendLine ("      _SnowTraxTextureBlend(\"Texture Blend\", Range(0, 1)) = 1");
            sb.AppendLine ("      _SnowTraxNormalStrength(\"Normal Shape Strength\", Range(0, 2.5)) = 1");

            sb.AppendLine ("      _TraxSnowErosion(\"Trax Snow Erosion\", Range(0,1)) = 0");
            sb.AppendLine ("      _TraxSnowHeight(\"Trax Snow Height\", Range(0,1)) = 0");
            sb.AppendLine ("      _TraxSnowAge(\"Trax Snow Crystals\", Float) = 0.8");
            sb.AppendLine ("      _TraxSnowRemoval(\"Trax Snow Removal\", Range(0, 1)) = 0");
            

         }
         // everyone gets this
         if (texMode != TraxTextureMode.None)
         {
            sb.AppendLine ("      _TraxNormalStrength(\"Normal Shape Strength\", Range(0, 2.5)) = 1");
         }

         if (texMode == TraxTextureMode.Single)
         {
            sb.AppendLine ("      _TraxDiff(\"Track Diffuse\", 2D) = \"white\" {}");
            sb.AppendLine ("      _TraxNSAO(\"Track NSAO\", 2D) = \"white\" {}");
            sb.AppendLine ("      _TraxUVScales(\"UV scale\", Vector) = (1,1,0,0)");
            sb.AppendLine ("      _TraxTextureBlend(\"Texture Blend\", Range(0, 1)) = 1");
            sb.AppendLine ("      _TraxInterpContrast(\"Interpolation Contrast\", Range(0, 1)) = 0.7");
         }
         if (texMode == TraxTextureMode.Array)
         {
            sb.AppendLine ("      _TraxArrayDiff(\"Track Diffuse\", 2DArray) = \"white\" {}");
            sb.AppendLine ("      _TraxArrayNSAO(\"Track NSAO\", 2DArray) = \"white\" {}");
            sb.AppendLine ("      _TraxUVScales(\"UV scale\", Vector) = (1,1,0,0)");
            sb.AppendLine ("      _TraxTextureBlend(\"Texture Blend\", Range(0, 1)) = 1");
            sb.AppendLine ("      _TraxInterpContrast(\"Interpolation Contrast\", Range(0, 1)) = 0.7");

         }
      }

      public override void WritePerMaterialCBuffer (string [] features, System.Text.StringBuilder sb)
      {
         if (texMode != TraxTextureMode.None || snowFootsteps)
         {
            sb.Append (cbuffer.text);
         }
      }

      public override void WriteSharedFunctions (string[] features, StringBuilder sb)
      {
         if (texMode != TraxTextureMode.None || snowFootsteps)
         {
            sb.AppendLine (func.text);
         }
      }

      public override void WriteFunctions (string[] features, System.Text.StringBuilder sb)
      {
         
      }

      public override void ComputeSampleCounts (string [] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {
         if (texMode != TraxTextureMode.None || (snowFootsteps && HasFeature (features, "_SNOW")))
         {
            // this is a lie, but is reasonably accurate perforamnce wise since all
            // samples come from the same place. Might even be generous..
            textureSampleCount += 3; 
         }
         if (snowFootsteps)
         {
            textureSampleCount += 2;
         }
         if (texMode == TraxTextureMode.Single)
         {
            textureSampleCount += 2;
         }
         if (texMode == TraxTextureMode.Array)
         {
            textureSampleCount += 8;
         }
      }

      static GUIContent CPerTexDigDepth = new GUIContent ("Trax Dig Depth", "How far tessellation is pushed down for the imprints");
      static GUIContent CPerTexOpacity = new GUIContent ("Trax Opacity", "How much is the effect applied to this texture");
      static GUIContent CPerTexNormalStr = new GUIContent ("Trax Normal Strength", "How strong is the lighting effect for the imprints on this texture");
      static GUIContent CPerTexTint = new GUIContent ("Trax Tint", "Tint color for trax");

      public override void DrawPerTextureGUI (int index, MicroSplatKeywords keywords, Material mat, MicroSplatPropData propData)
      {
         if (texMode != TraxTextureMode.None)
         {
            InitPropData (20, propData, new Color (1, 1, 1, 1));
            InitPropData (21, propData, new Color (0.5f, 0.5f, 0.5f, 1));
            perTexOpacity = DrawPerTexFloatSlider (index, 20, GetFeatureName (DefineFeature._PERTEXTRAXOPACITY), keywords, propData, Channel.G, CPerTexOpacity, 0, 1);
            perTexNormalStr = DrawPerTexFloatSlider (index, 20, GetFeatureName (DefineFeature._PERTEXTRAXNORMALSTR), keywords, propData, Channel.B, CPerTexNormalStr, 0, 2);
            //// 20 Trax Dig Depth, opacity, normal strength (A) Open
            if (keywords.IsKeywordEnabled ("_TESSDISTANCE"))
            {
               perTexDigDepth = DrawPerTexFloatSlider (index, 20, GetFeatureName (DefineFeature._PERTEXTRAXDIGDEPTH), keywords, propData, Channel.R, CPerTexDigDepth, 0, 3);
            }

            perTexTraxTint = DrawPerTexColor (index, 21, GetFeatureName (DefineFeature._PERTEXTRAXTINT), keywords, propData, CPerTexTint, false);
         }
      }
   }
#endif

}
