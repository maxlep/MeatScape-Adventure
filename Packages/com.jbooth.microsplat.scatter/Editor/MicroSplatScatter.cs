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
   public class MicroSplatScatter : FeatureDescriptor
   {
      const string sDefine = "__MICROSPLAT_SCATTER__";
      static MicroSplatScatter ()
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
         return "Scatter";
      }

      public override string GetHelpPath ()
      {
         return "/MicroSplat/Core/Documentation/MicroSplat - Scatter.pdf";
      }

      public enum DefineFeature
      {
         _SCATTER,
         _PERTEXSCATTERUV,
         _PERTEXSCATTERHEIGHTFILTER,
         _PERTEXSCATTERSLOPEFILTER,
         _PERTEXSCATTERBLENDMODE,
         _PERTEXSCATTERALPHABOOST,
         _PERTEXSCATTERFADE,
         _SPLATTERSECONDLAYER,
         kNumFeatures,
      }

      public enum ScatterMode
      {
         Off,
         SingleLayer,
         TwoLayer
      }

      ScatterMode scatter = ScatterMode.Off;
      
      bool perTexUV;
      bool perTexHeightFilter;
      bool perTexSlopeFilter;
      bool perTexBlendMode;
      bool perTexScatterAlphaBoost;
      bool perTexScatterFade;

      static TextAsset properties;
      static TextAsset funcs;
      static TextAsset cbuffer;

      public enum BlendModes
      {
         Alpha,
         AlphaClip,
         Overlay,
         LighterColor
      }

      static GUIContent [] BlendNames =
      {
         new GUIContent("Alpha", "Alpha blend based on provided alpha channel"),
         new GUIContent("AlphaClip", "Filters based on alpha amount. With this, you can have each scatter object have a different alpha, and they will appear or not based on the threshold"),
         new GUIContent("Overlay", "Overlay Blend mode, like Photoshop"),
         new GUIContent("Lighter Color", "Lighter color wins"),
      };



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

      public override string GetVersion ()
      {
         return "3.8";
      }

      static GUIContent CScatter = new GUIContent ("Scatter", "Turn on Scatter layer painting");
      static GUIContent ptUVScale = new GUIContent ("UV Scale", "UV scale for scatter");
      static GUIContent ptHeightRangeFilter = new GUIContent ("Height Filter", "Lets you filter splatter only on certain heights of the underlying texture. If you make the min > max, the filter is inverted.");
      static GUIContent ptSlopeRangeFilter = new GUIContent ("Slope Range Filter", "Slope filter, so you can have it only appear on sides");
      static GUIContent ptBlendMode = new GUIContent ("Blend Mode", "Allows you to control how this is blended with the texture");
      static GUIContent ptAlphaBoost = new GUIContent ("Alpha Multiplier", "Allows you to scale up/down the alpha channel");
      static GUIContent ptAlphaFade = new GUIContent ("Alpha Fade", "Allows you to fade out the scatter based on the mip level used. This is useful when the lower mips of the texture turn the scatter into a solid color, changing the overall color of the terrain.");

      public override void DrawFeatureGUI (MicroSplatKeywords keywords)
      {
         scatter = (ScatterMode)EditorGUILayout.EnumPopup (CScatter, scatter);
      }

      static GUIContent CDiffuse = new GUIContent ("Scatter Diffuse", "Diffuse/Alpha array for scatter");
      static GUIContent CNormal = new GUIContent ("Scatter NSAO", "Normal, Smoothness, AO array for scatter");
      static GUIContent CUVScale = new GUIContent ("UV Scale", "UV scale for scatter");



      int perTexIndex = 0;

      public void DrawPerTex(MicroSplatKeywords keywords, Material mat, int perTexIndex, MicroSplatPropData propData)
      {
         perTexUV = DrawPerTexVector2 (perTexIndex, 24, "_PERTEXSCATTERUV", keywords, propData, V2Cannel.RG, ptUVScale);
         perTexScatterAlphaBoost = DrawPerTexFloatSlider (perTexIndex, 24, "_PERTEXSCATTERALPHABOOST", keywords, propData, Channel.A, ptAlphaBoost, 0, 2);
         perTexBlendMode = DrawPerTexPopUp (perTexIndex, 24, "_PERTEXSCATTERBLENDMODE", keywords, propData, Channel.B, ptBlendMode, BlendNames);
         perTexHeightFilter = DrawPerTexVector2 (perTexIndex, 25, "_PERTEXSCATTERHEIGHTFILTER", keywords, propData, V2Cannel.RG, ptHeightRangeFilter);
         perTexSlopeFilter = DrawPerTexVector2 (perTexIndex, 25, "_PERTEXSCATTERSLOPEFILTER", keywords, propData, V2Cannel.BA, ptSlopeRangeFilter);
         perTexScatterFade = DrawPerTexFloatSlider (perTexIndex, 26, "_PERTEXSCATTERFADE", keywords, propData, Channel.R, ptAlphaFade, 0, 6);
      }

      // for the painting interface
      public static bool DrawPerTexExternal (Material mat, int perTexIndex)
      {
         MicroSplatKeywords keywords = MicroSplatUtilities.FindOrCreateKeywords (mat);
         var propData = MicroSplatShaderGUI.FindOrCreatePropTex (mat);
         if (keywords != null && propData != null)
         {
            bool oldUV = keywords.IsKeywordEnabled ("_PERTEXSCATTERUV");
            bool oldAlpha = keywords.IsKeywordEnabled ("_PERTEXSCATTERALPHABOOST");
            bool oldBlend = keywords.IsKeywordEnabled ("_PERTEXSCATTERBLENDMODE");
            bool oldHeight = keywords.IsKeywordEnabled ("_PERTEXSCATTERHEIGHTFILTER");
            bool oldSlope = keywords.IsKeywordEnabled ("_PERTEXSCATTERSLOPEFILTER");
            bool oldFade = keywords.IsKeywordEnabled ("_PERTEXSCATTERFADE");

            bool uv = DrawPerTexVector2 (perTexIndex, 24, "_PERTEXSCATTERUV", keywords, propData, V2Cannel.RG, ptUVScale);
            bool alpha = DrawPerTexFloatSlider (perTexIndex, 24, "_PERTEXSCATTERALPHABOOST", keywords, propData, Channel.A, ptAlphaBoost, 0, 2);
            bool blend = DrawPerTexPopUp (perTexIndex, 24, "_PERTEXSCATTERBLENDMODE", keywords, propData, Channel.B, ptBlendMode, BlendNames);
            bool height = DrawPerTexVector2 (perTexIndex, 25, "_PERTEXSCATTERHEIGHTFILTER", keywords, propData, V2Cannel.RG, ptHeightRangeFilter);
            bool slope = DrawPerTexVector2 (perTexIndex, 25, "_PERTEXSCATTERSLOPEFILTER", keywords, propData, V2Cannel.BA, ptSlopeRangeFilter);
            bool fade = DrawPerTexFloatSlider (perTexIndex, 26, "_PERTEXSCATTERFADE", keywords, propData, Channel.R, ptAlphaFade, 0, 6);

            return oldUV != uv || oldAlpha != alpha || oldBlend != blend || oldHeight != height || oldSlope != slope || oldFade != fade;
         }
         return false;
      }

      static GUIContent CUVScale2 = new GUIContent ("Second Layer UV scale/offset", "Scale and offset for second layer");

      public override void DrawShaderGUI (MicroSplatShaderGUI shaderGUI, MicroSplatKeywords keywords, Material mat, MaterialEditor materialEditor, MaterialProperty [] props)
      {
         if (scatter != ScatterMode.Off && mat.HasProperty ("_ScatterControl"))
         {
            if (MicroSplatUtilities.DrawRollup ("Scatter"))
            {
               var albedoMap = shaderGUI.FindProp ("_ScatterDiffuse", props);
               var normalMap = shaderGUI.FindProp ("_ScatterNSAO", props);
               materialEditor.TexturePropertySingleLine (CDiffuse, albedoMap);
               materialEditor.TexturePropertySingleLine (CNormal, normalMap);

               Vector2 uvScale = mat.GetVector ("_ScatterUVScale");
               Vector2 nuv = EditorGUILayout.Vector2Field (CUVScale, uvScale);
               if (nuv != uvScale)
               {
                  mat.SetVector ("_ScatterUVScale", nuv);
                  EditorUtility.SetDirty (mat);
               }

               if (scatter == ScatterMode.TwoLayer && mat.HasProperty ("_ScatterUVScale2"))
               {
                  Vector4 uv2Scale = mat.GetVector ("_ScatterUVScale2");
                  Vector4 nuv2 = EditorGUILayout.Vector4Field (CUVScale2, uv2Scale);
                  if (uv2Scale != nuv2)
                  {
                     mat.SetVector ("_ScatterUVScale2", nuv2);
                     EditorUtility.SetDirty (mat);
                  }
               }

               var propData = MicroSplatShaderGUI.FindOrCreatePropTex (mat);
               if (propData != null)
               {
                  // UV Scale, blend mode, open
                  InitPropData (24, propData, new Color (1, 1, 0, 1));
                  // height and slope filter
                  InitPropData (25, propData, new Color (0, 1, 0, 1));
                  // scatter fade
                  InitPropData (26, propData, new Color (0,0,0,1));

                  perTexIndex = MicroSplatUtilities.DrawTextureSelector (perTexIndex, (Texture2DArray)albedoMap.textureValue, false);

                  DrawPerTex (keywords, mat, perTexIndex, propData);
                  
               }
            }
         }

      }

      public override string [] Pack ()
      {
         List<string> features = new List<string> ();
         if (scatter != ScatterMode.Off)
         {
            features.Add (GetFeatureName (DefineFeature._SCATTER));
            if (scatter == ScatterMode.TwoLayer)
            {
               features.Add (GetFeatureName (DefineFeature._SPLATTERSECONDLAYER));
            }
         }
         if (perTexUV)
         {
            features.Add (GetFeatureName (DefineFeature._PERTEXSCATTERUV));
         }
         if (perTexHeightFilter)
         {
            features.Add (GetFeatureName (DefineFeature._PERTEXSCATTERHEIGHTFILTER));
         }
         if (perTexBlendMode)
         {
            features.Add (GetFeatureName (DefineFeature._PERTEXSCATTERBLENDMODE));
         }
         if (perTexSlopeFilter)
         {
            features.Add (GetFeatureName (DefineFeature._PERTEXSCATTERSLOPEFILTER));
         }
         if (perTexScatterAlphaBoost)
         {
            features.Add (GetFeatureName (DefineFeature._PERTEXSCATTERALPHABOOST));
         }
         if (perTexScatterFade)
         {
            features.Add (GetFeatureName (DefineFeature._PERTEXSCATTERFADE));
         }
        
         return features.ToArray ();
      }

      public override void Unpack (string [] keywords)
      {
         scatter = ScatterMode.Off;
         if (HasFeature (keywords, DefineFeature._SCATTER))
         {
            scatter = ScatterMode.SingleLayer;
            if (HasFeature(keywords, DefineFeature._SPLATTERSECONDLAYER))
            {
               scatter = ScatterMode.TwoLayer;
            }
         }
         

         perTexUV = HasFeature (keywords, DefineFeature._PERTEXSCATTERUV);
         perTexHeightFilter = HasFeature (keywords, DefineFeature._PERTEXSCATTERHEIGHTFILTER);
         perTexBlendMode = HasFeature (keywords, DefineFeature._PERTEXSCATTERBLENDMODE);
         perTexSlopeFilter = HasFeature (keywords, DefineFeature._PERTEXSCATTERSLOPEFILTER);
         perTexScatterAlphaBoost = HasFeature (keywords, DefineFeature._PERTEXSCATTERALPHABOOST);
         perTexScatterFade = HasFeature (keywords, DefineFeature._PERTEXSCATTERFADE);
      }

      public override void InitCompiler (string [] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths [i];
            if (p.EndsWith ("microsplat_properties_scatter.txt"))
            {
               properties = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
            if (p.EndsWith ("microsplat_func_scatter.txt"))
            {
               funcs = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
            if (p.EndsWith ("microsplat_cbuffer_scatter.txt"))
            {
               cbuffer = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
         }
      }

      public override void WriteProperties (string [] features, System.Text.StringBuilder sb)
      {
         if (scatter != ScatterMode.Off)
         {
            sb.Append (properties.text);
         }
      }

      public override void WriteFunctions (string [] features, System.Text.StringBuilder sb)
      {
         if (scatter != ScatterMode.Off)
         {
            sb.Append (funcs.text);
         }
      }

      public override void WritePerMaterialCBuffer (string [] features, System.Text.StringBuilder sb)
      {
         if (scatter != ScatterMode.Off)
         {
            sb.Append (cbuffer.text);
         }
      }

      

      public override void ComputeSampleCounts (string [] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {
         if (scatter != ScatterMode.Off)
         {
            textureSampleCount += 3;
            arraySampleCount += 6;
            if (scatter == ScatterMode.TwoLayer)
            {
               arraySampleCount += 6;
            }
         }

      }

   }

#endif
}