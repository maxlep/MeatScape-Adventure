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
   public class MicroSplatGlobalTexture : FeatureDescriptor
   {
      const string sGlobalTextureDefine = "__MICROSPLAT_GLOBALTEXTURE__";
      static MicroSplatGlobalTexture()
      {
         MicroSplatDefines.InitDefine(sGlobalTextureDefine);
      }
      [PostProcessSceneAttribute(0)]
      public static void OnPostprocessScene()
      {
         MicroSplatDefines.InitDefine(sGlobalTextureDefine);
      }

      public override string ModuleName()
      {
         return "Global Texturing";
      }

      public override string GetHelpPath ()
      {
         return "/MicroSplat/Core/Documentation/MicroSplat - Global Texturing.pdf";
      }

      public enum DefineFeature
      {
         _GEOMAP,
         _GEONORMAL,
         _GEORANGE,
         _GEOCURVE,
         _GEOSLOPEFILTER,
         _GEOTEXLIGHTCOLOR,
         _PERTEXGEO,
         _GLOBALTINT,
         _GLOBALTINTBIQUADRATIC,
         _GLOBALNORMALS,
         _GLOBALNORMALSBIQUADRATIC,
         _GLOBALSMOOTHAOMETAL,
         _GLOBALSAOMBIQUADRATIC,
         _GLOBALSPECULAR,
         _GLOBALSPECBIQUADRATIC,
         _GLOBALEMIS,
         _GLOBALEMISBIQUADRATIC,
         _GLOBALTINTMULT2X,
         _GLOBALTINTOVERLAY,
         _GLOBALTINTCROSSFADE,
         _GLOBALTINTLIGHTCOLOR,
         _GLOBALSPECULARMULT2X,
         _GLOBALSPECULAROVERLAY,
         _GLOBALSPECULARCROSSFADE,
         _GLOBALSPECULARLIGHTCOLOR,
         _GLOBALNORMALCROSSFADE,
         _GLOBALSLOPEFILTER,
         _GLOBALNOISEUV,

         _PERTEXGLOBALTINTSTRENGTH,
         _PERTEXGLOBALNORMALSTRENGTH,
         _PERTEXGLOBALSOAMSTRENGTH,
         _PERTEXGLOBALSPECULARSTRENGTH,
         _PERTEXGLOBALEMISSTRENGTH,
         _PERTEXGEOMAPHEIGHT,
         _PERTEXGEOMAPHEIGHTSTRENGTH,
         _GLOBALNORMALPACKEDSAO,
         _SPLATFADE,

         _GLOBALTEXWRAP,
         kNumFeatures,
      }


      public enum BlendMode
      {
         Off,
         Multiply2X,
         Overlay,
         LightColor,
         CrossFade
      }

      public enum NormalBlendMode
      {
         Off,
         NormalBlend,
         CrossFade
      }

      public enum SAOMBlendMode
      {
         Off,
         CrossFade
      }

      public enum NormalPackMode
      {
         Separate,
         Packed
      }

      public enum GeoTextureMode
      {
         Off,
         Albedo,
         AlbedoNormal,
      }

      public enum GeoTextureBlendMode
      {
         Multiply2X,
         LightColor
      }

      public enum SamplingMode
      {
         Linear,
         Quadratic
      }


      public enum PresetNames
      {
         Floor,
         Slopes,
         Cliff
      }

      public enum WrapMode
      {
         Clamp,
         Wrap
      }

      static AnimationCurve[] slopePresets;
      PresetNames currentPreset = PresetNames.Cliff;

      void InitPresets()
      {
         if (slopePresets == null)
         {
            slopePresets = new AnimationCurve[3];
            slopePresets [0] = AnimationCurve.EaseInOut (0.0f, 1.0f, 0.03f, 0);
            slopePresets[1] = new AnimationCurve(new Keyframe[4] { new Keyframe(0.0f, 0), new Keyframe(0.17f, 1), new Keyframe(0.21f, 1), new Keyframe(0.41f, 0) });
            slopePresets[2] = new AnimationCurve(new Keyframe[2] { new Keyframe(0.18f, 0), new Keyframe(0.25f, 1)});
         }
      }

      class PresetSelection
      {
         public PresetSelection(PresetNames n, MicroSplatPropData p)
         {
            propData = p;
            name = n;
         }
         public MicroSplatPropData propData;
         public PresetNames name;
      }

      void OnGeoPresetSelected(object obj)
      {
         PresetSelection ps = (PresetSelection)obj;
         currentPreset = ps.name;
         ps.propData.geoSlopeFilter = slopePresets[(int)currentPreset];
         MicroSplatObject.SyncAll();
      }

      void OnGlobalPresetSelected(object obj)
      {
         PresetSelection ps = (PresetSelection)obj;
         currentPreset = ps.name;
         ps.propData.globalSlopeFilter = slopePresets[(int)currentPreset];
         MicroSplatObject.SyncAll();
      }

      public GeoTextureMode geoTexture = GeoTextureMode.Off;
      public GeoTextureBlendMode geoTextureBlendMode = GeoTextureBlendMode.Multiply2X;

      public bool geoRange;
      public bool perTexGeoStr;
      public bool perTexTintStr;
      public bool perTexNormalStr;
      public bool perTexEmisStr;
      public bool perTexSAOMStr;
      public bool perTexSpecularStr;
      public bool geoCurve;
      public bool geoSlopeFilter;
      public bool perTexGeoStrength;
      public bool perTexGeoHeight;
      public bool perTexGeoHeightStrength;
      public bool globalSlopeFilter;
      public bool splatFade;
      public bool globalNoiseUV;
      public SamplingMode tintSampleMode = SamplingMode.Linear;
      public SamplingMode normalSampleMode = SamplingMode.Linear;
      public SamplingMode specularSampleMode = SamplingMode.Linear;
      public SamplingMode emisSampleMode = SamplingMode.Linear;
      public SamplingMode saomSampleMode = SamplingMode.Linear;

      public NormalPackMode normalPackMode = NormalPackMode.Separate;
      public WrapMode wrapMode = WrapMode.Clamp;

      public BlendMode tintBlendMode = BlendMode.Off;
      public NormalBlendMode normalBlendMode = NormalBlendMode.Off;
      public SAOMBlendMode SAOMBlend = SAOMBlendMode.Off;
      public SAOMBlendMode emisBlend = SAOMBlendMode.Off;
      public BlendMode specularBlendMode = BlendMode.Off;

      public TextAsset properties_geomap;
      public TextAsset properties_tint;
      public TextAsset properties_normal;
      public TextAsset properties_saom;
      public TextAsset properties_emis;
      public TextAsset properties_params;
      public TextAsset properties_specular;
      public TextAsset function_geomap;
      public TextAsset cbuffer_geomap;

      GUIContent CShaderGeoTexture = new GUIContent("Geo Height Texture", "Enabled Geo Height Texture, which is mapped virtically to colorize the terrain");
      GUIContent CGeoTextureBlendMode = new GUIContent ("Geo Blend Mode", "How geo texture albedo is blended with the terrain");
      GUIContent CShaderGeoRange = new GUIContent("Geo Range", "Fade geo effect so it only affects a certain height range of the terrain");
      GUIContent CShaderGeoCurve = new GUIContent("Geo Curve", "Use a Curve to distort the height of the geotexture on the terrain");
      GUIContent CGeoSlopeFilter = new GUIContent ("Geo Slope Filter", "Use a curve to filter the effect of the geo texture based on slope of the terrain");
      GUIContent CShaderTint = new GUIContent("Global Tint", "Enable a Tint map, which is blended with the albedo of the terrain in one of several ways");
      GUIContent CShaderGlobalNormal = new GUIContent("Global Normal", "Enabled a global normal map which is blended with the terrain in one of several ways");
      GUIContent CShaderGlobalSAOM = new GUIContent("Global Smoothness(R), AO(G), Metallic(B)", "Global map for smoothness, ao, and metallic values");
      GUIContent CShaderGlobalSpec = new GUIContent("Global Specular", "Global map for specular color");
      GUIContent CShaderGlobalEmis = new GUIContent("Global Emissive Map", "Global map for emissive color");
      GUIContent CShaderGeoHeightContrast = new GUIContent("Height Filter Contrast", "Controls the contrast of the per-texture height filter");
      GUIContent CShaderNormalPackMode = new GUIContent("Global Normal Pack Mode", "In Packed mode, separate Smoothness/AO map is not used and instead it's expected to be packed into the R and B channels of the normal map, with normal in G/A");
      GUIContent CShaderGlobalSlopeFilter = new GUIContent("Global Slope Filter", "Allows you to adjust strength of global texturing based on a slope curve");
      GUIContent CSplatFade = new GUIContent ("Splat Fade Distance", "Allows you to crossfade from splat maps to a single texture in the distance. After that point, splat maps are no longer computed or samples. ");
      GUIContent CSamplingMode = new GUIContent ("Sampling Mode", "Use more samples to increase the quality of the filtering for better color interpolation");
      GUIContent CNoiseUV = new GUIContent ("Global Noise UV", "Apply noise to the UV coordinates to break up low res look of global texturing");
      GUIContent CWrapMode = new GUIContent ("Wrap Mode", "Should the texture wrap or clamp?");
      // Can we template these somehow?
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
         geoTexture = (GeoTextureMode)EditorGUILayout.EnumPopup(CShaderGeoTexture, geoTexture);
         if (geoTexture != GeoTextureMode.Off)
         {
            EditorGUI.indentLevel++;
            geoTextureBlendMode = (GeoTextureBlendMode)EditorGUILayout.EnumPopup (CGeoTextureBlendMode, geoTextureBlendMode);
            geoRange = EditorGUILayout.Toggle(CShaderGeoRange, geoRange);
            geoCurve = EditorGUILayout.Toggle(CShaderGeoCurve, geoCurve);
            geoSlopeFilter = EditorGUILayout.Toggle(CGeoSlopeFilter, geoSlopeFilter);
            EditorGUI.indentLevel--;
         }
         tintBlendMode = (BlendMode)EditorGUILayout.EnumPopup(CShaderTint, tintBlendMode);
         if (tintBlendMode != BlendMode.Off)
         {
            EditorGUI.indentLevel++;
            tintSampleMode = (SamplingMode)EditorGUILayout.EnumPopup (CSamplingMode, tintSampleMode);
            EditorGUI.indentLevel--;
         }
         normalBlendMode = (NormalBlendMode)EditorGUILayout.EnumPopup(CShaderGlobalNormal, normalBlendMode);
         if (normalBlendMode != NormalBlendMode.Off)
         {
            EditorGUI.indentLevel++;
            normalPackMode = (NormalPackMode)EditorGUILayout.EnumPopup(CShaderNormalPackMode, normalPackMode);
            normalSampleMode = (SamplingMode)EditorGUILayout.EnumPopup (CSamplingMode, normalSampleMode);
            EditorGUI.indentLevel--;
         }
         if (normalPackMode != NormalPackMode.Packed)
         {
            SAOMBlend = (SAOMBlendMode)EditorGUILayout.EnumPopup(CShaderGlobalSAOM, SAOMBlend);
            if (SAOMBlend != SAOMBlendMode.Off)
            {
               EditorGUI.indentLevel++;
               saomSampleMode = (SamplingMode)EditorGUILayout.EnumPopup (CSamplingMode, saomSampleMode);
               EditorGUI.indentLevel--;
            }
         }
         if (keywords.IsKeywordEnabled("_USESPECULARWORKFLOW"))
         {
            specularBlendMode = (BlendMode)EditorGUILayout.EnumPopup (CShaderGlobalSpec, specularBlendMode);
            if (specularBlendMode != BlendMode.Off)
            {
               EditorGUI.indentLevel++;
               specularSampleMode = (SamplingMode)EditorGUILayout.EnumPopup (CSamplingMode, specularSampleMode);
               EditorGUI.indentLevel--;
            }
         }
         emisBlend = (SAOMBlendMode)EditorGUILayout.EnumPopup(CShaderGlobalEmis, emisBlend);
         if (emisBlend != SAOMBlendMode.Off)
         {
            EditorGUI.indentLevel++;
            emisSampleMode = (SamplingMode)EditorGUILayout.EnumPopup (CSamplingMode, emisSampleMode);
            EditorGUI.indentLevel--;
         }
         if (tintBlendMode != BlendMode.Off || normalBlendMode != NormalBlendMode.Off || emisBlend != SAOMBlendMode.Off || SAOMBlend != SAOMBlendMode.Off)
         {
            globalSlopeFilter = EditorGUILayout.Toggle(CShaderGlobalSlopeFilter, globalSlopeFilter);
            globalNoiseUV = EditorGUILayout.Toggle (CNoiseUV, globalNoiseUV);
            wrapMode = (WrapMode) EditorGUILayout.EnumPopup (CWrapMode, wrapMode);
         }

        

         splatFade = EditorGUILayout.Toggle (CSplatFade, splatFade);

      }

      static GUIContent CGeoTex = new GUIContent("Geo Texture", "Virtical striping texture for terrain");
      static GUIContent CGeoNormal = new GUIContent("Geo Normal", "Virtical striping texture for terrain");
      static GUIContent CGeoRange = new GUIContent("Geo Range", "World height at which geo texture begins to fade in, is faded in completely, begins to fade out, and is faded out completely");
      static GUIContent CGeoCurve = new GUIContent("Height Curve", "Allows you to bend the height lookup on the GeoTexture");
      static GUIContent CGeoSlope = new GUIContent ("Slope Filter", "Filter angles at which this effect appears. Slope is the horizonal value, weight is the virtical, with 0.5 being cliffs and 1 being flats");
      static GUIContent CUVScale = new GUIContent("UV Scale", "Scale for UV tiling");
      static GUIContent CUVOffset = new GUIContent("UV Offset", "Offset for UV tiling");
      static GUIContent CNormalMap = new GUIContent("Normal Texture", "Normal texture for global normals");
      static GUIContent CNormalSAOMap = new GUIContent("Normal SAO Texture", "Normal (G/A) with smoothness (R) and AO (B)");
      static GUIContent CSplatFadeDist = new GUIContent ("Splat Fade Begin/End", "Range at which to fade out splat mapping based on start/end distance");
      
      public override void DrawShaderGUI(MicroSplatShaderGUI shaderGUI, MicroSplatKeywords keywords, Material mat, MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (splatFade && MicroSplatUtilities.DrawRollup("Splat Fade"))
         {
            if (mat.HasProperty ("_SplatFade") && mat.HasProperty ("_Diffuse"))
            {
               var sf = shaderGUI.FindProp ("_SplatFade", props);
               Vector4 fade = sf.vectorValue;
               int textureIndex = (int)fade.z;
               Vector2 fs = new Vector2 (fade.x, fade.y);
               Texture2DArray ta = mat.GetTexture ("_Diffuse") as Texture2DArray;


               int nti = MicroSplatUtilities.DrawTextureSelector (textureIndex, ta, true);
               fs = EditorGUILayout.Vector2Field (CSplatFadeDist, fs);

               if (fs.x != fade.x || fs.y != fade.y || (int)nti != textureIndex)
               {
                  sf.vectorValue = new Vector4 (fs.x, fs.y, nti, fade.w);
               }
            }
         }

         if (geoTexture != GeoTextureMode.Off && MicroSplatUtilities.DrawRollup("Geo Texture"))
         {
            
            if (mat.HasProperty("_GeoTex"))
            {
               var texProp = shaderGUI.FindProp("_GeoTex", props);
               materialEditor.TexturePropertySingleLine(CGeoTex, texProp);
               MicroSplatUtilities.EnforceDefaultTexture(texProp, "microsplat_def_geomap_01");

               if (geoTexture != GeoTextureMode.Albedo && mat.HasProperty("_GeoNormal"))
               {
                  var texNormalProp = shaderGUI.FindProp("_GeoNormal", props);
                  materialEditor.TexturePropertySingleLine(CGeoNormal, texNormalProp);
                  MicroSplatUtilities.EnforceDefaultTexture(texNormalProp, "microsplat_def_geomap_norm_01");
                  materialEditor.RangeProperty(shaderGUI.FindProp("_GeoNormalStrength", props), "Normal Strength");
               }

               Vector4 parms = mat.GetVector("_GeoParams");
               EditorGUI.BeginChangeCheck();
               parms.x = EditorGUILayout.Slider("Blend", parms.x, 0, 1);
               parms.y = 1.0f / Mathf.Max(parms.y, 0.00001f);
               parms.y = EditorGUILayout.FloatField("World Scale", parms.y);
               parms.y = 1.0f / Mathf.Max(parms.y, 0.00001f);
               parms.z = EditorGUILayout.FloatField("World Offset", parms.z);
               if (EditorGUI.EndChangeCheck())
               {
                  mat.SetVector("_GeoParams", parms);
                  EditorUtility.SetDirty(mat);
               }
               if (geoRange && mat.HasProperty("_GeoRange"))
               {
                  Vector4 rangeParams = mat.GetVector("_GeoRange");
                  EditorGUI.BeginChangeCheck();
                  rangeParams = EditorGUILayout.Vector4Field(CGeoRange, rangeParams);
                  if (EditorGUI.EndChangeCheck())
                  {
                     if (rangeParams.z < rangeParams.x || rangeParams.z < rangeParams.y)
                     {
                        rangeParams.z = rangeParams.y;
                     }
                     if (rangeParams.y < rangeParams.x)
                     {
                        rangeParams.y = rangeParams.x;
                     }
                     if (rangeParams.w < rangeParams.z)
                     {
                        rangeParams.z = rangeParams.w;
                     }

                     mat.SetVector("_GeoRange", rangeParams);
                     EditorUtility.SetDirty(mat);
                  }
               }
               if (geoCurve && mat.HasProperty("_GeoCurveParams"))
               {
                  var propData = MicroSplatShaderGUI.FindOrCreatePropTex(mat);
                  EditorGUI.BeginChangeCheck();
                  if (propData != null)
                  {
                     propData.geoCurve = EditorGUILayout.CurveField(CGeoCurve, propData.geoCurve);
                  }
                  Vector4 curveParams = mat.GetVector("_GeoCurveParams");
                  curveParams.x = EditorGUILayout.FloatField("Scale", curveParams.x);
                  curveParams.y = EditorGUILayout.FloatField("Offset", curveParams.y);
                  curveParams.z = EditorGUILayout.FloatField("Rotation", curveParams.z);

                  if (EditorGUI.EndChangeCheck())
                  {
                     AnimationCurve c = propData.geoCurve;
                     for (int i = 0; i < c.length; ++i)
                     {
                        c.keys[i].time = Mathf.Clamp01(c.keys[i].time);
                     }
                     mat.SetVector("_GeoCurveParams", curveParams);
                     EditorUtility.SetDirty(mat);
                     EditorUtility.SetDirty(propData);
                     MicroSplatObject.SyncAll();
                  }
               }
               if (geoSlopeFilter && mat.HasProperty ("_GeoSlopeTex"))
               {
                  
                  var propData = MicroSplatShaderGUI.FindOrCreatePropTex(mat);
                  EditorGUI.BeginChangeCheck();
                  if (propData != null)
                  {
                     EditorGUILayout.BeginHorizontal ();
                     propData.geoSlopeFilter = EditorGUILayout.CurveField(CGeoSlope, propData.geoSlopeFilter, Color.white, new Rect(0,0,1,1));
                     if (GUILayout.Button ("P", GUILayout.Width (22)))
                     {
                        InitPresets ();
                        GenericMenu menu = new GenericMenu ();
                        menu.AddItem(new GUIContent(PresetNames.Floor.ToString()), currentPreset == PresetNames.Floor, OnGeoPresetSelected, new PresetSelection(PresetNames.Floor, propData));
                        menu.AddItem(new GUIContent(PresetNames.Slopes.ToString()), currentPreset == PresetNames.Slopes, OnGeoPresetSelected, new PresetSelection(PresetNames.Slopes, propData));
                        menu.AddItem(new GUIContent(PresetNames.Cliff.ToString()), currentPreset == PresetNames.Cliff, OnGeoPresetSelected, new PresetSelection(PresetNames.Cliff, propData));
                        menu.ShowAsContext();
                     }
                     EditorGUILayout.EndHorizontal ();
                  }

                  if (EditorGUI.EndChangeCheck())
                  {
                     EditorUtility.SetDirty(mat);
                     EditorUtility.SetDirty(propData);
                     MicroSplatObject.SyncAll();
                  }
               }
               if (perTexGeoHeight && mat.HasProperty("_GeoHeightContrast"))
               {
                  EditorGUI.BeginChangeCheck();

                  float v = mat.GetFloat("_GeoHeightContrast");
                  v = EditorGUILayout.Slider(CShaderGeoHeightContrast, v, 0, 1);

                  if (EditorGUI.EndChangeCheck())
                  {
                     mat.SetFloat("_GeoHeightContrast", v);
                     EditorUtility.SetDirty(mat);
                  }
               }

            }
         }
         if ((tintBlendMode != BlendMode.Off || normalBlendMode != NormalBlendMode.Off || SAOMBlend != SAOMBlendMode.Off || emisBlend != SAOMBlendMode.Off || specularBlendMode != BlendMode.Off) && MicroSplatUtilities.DrawRollup("Global Texture"))
         {
            if (tintBlendMode != BlendMode.Off && mat.HasProperty("_GlobalTintTex"))
            {
               using (new GUILayout.VerticalScope (GUI.skin.box))
               {
                  materialEditor.TexturePropertySingleLine (new GUIContent ("Tint Texture", "Albedo Tint Texture"), shaderGUI.FindProp ("_GlobalTintTex", props));
                  Vector4 parms = mat.GetVector ("_GlobalTextureParams");
                  EditorGUI.BeginChangeCheck ();
                  parms.x = EditorGUILayout.Slider ("Blend", parms.x, 0, 1);
                  if (EditorGUI.EndChangeCheck ())
                  {
                     mat.SetVector ("_GlobalTextureParams", parms);
                     EditorUtility.SetDirty (mat);
                  }
                  if (mat.HasProperty ("_GlobalTintFade"))
                  {
                     Vector4 fade = mat.GetVector ("_GlobalTintFade");
                     EditorGUI.BeginChangeCheck ();

                     fade.x = EditorGUILayout.FloatField ("Begin Fade", fade.x);
                     fade.z = EditorGUILayout.Slider ("Opacity At Begin", fade.z, 0, 1);
                     fade.y = EditorGUILayout.FloatField ("Fade Range", fade.y);
                     fade.w = EditorGUILayout.Slider ("Opacity At End", fade.w, 0, 1);

                     if (EditorGUI.EndChangeCheck ())
                     {
                        mat.SetVector ("_GlobalTintFade", fade);
                        EditorUtility.SetDirty (mat);
                     }
                  }
                  if (mat.HasProperty ("_GlobalTintUVScale"))
                  {
                     Vector4 uv = mat.GetVector ("_GlobalTintUVScale");
                     Vector2 scale = new Vector2 (uv.x, uv.y);
                     Vector2 offset = new Vector2 (uv.z, uv.w);

                     EditorGUI.BeginChangeCheck ();
                     scale = EditorGUILayout.Vector2Field (CUVScale, scale);
                     offset = EditorGUILayout.Vector2Field (CUVOffset, offset);

                     if (EditorGUI.EndChangeCheck ())
                     {
                        uv = new Vector4 (scale.x, scale.y, offset.x, offset.y);
                        mat.SetVector ("_GlobalTintUVScale", uv);
                        EditorUtility.SetDirty (mat);
                     }
                  }
               }
            }
            if (normalBlendMode != NormalBlendMode.Off && mat.HasProperty("_GlobalNormalTex"))
            {
               using (new GUILayout.VerticalScope (GUI.skin.box))
               {
                  materialEditor.TexturePropertySingleLine (normalPackMode == NormalPackMode.Packed ? CNormalSAOMap : CNormalMap, shaderGUI.FindProp ("_GlobalNormalTex", props));
                  Vector4 parms = mat.GetVector ("_GlobalTextureParams");
                  EditorGUI.BeginChangeCheck ();
                  parms.y = EditorGUILayout.Slider ("Blend", parms.y, 0, 3);
                  if (EditorGUI.EndChangeCheck ())
                  {
                     mat.SetVector ("_GlobalTextureParams", parms);
                     EditorUtility.SetDirty (mat);
                  }

                  if (mat.HasProperty ("_GlobalNormalFade"))
                  {
                     Vector4 fade = mat.GetVector ("_GlobalNormalFade");
                     EditorGUI.BeginChangeCheck ();

                     fade.x = EditorGUILayout.FloatField ("Begin Fade", fade.x);
                     fade.z = EditorGUILayout.Slider ("Opacity At Begin", fade.z, 0, 1);
                     fade.y = EditorGUILayout.FloatField ("Fade Range", fade.y);
                     fade.w = EditorGUILayout.Slider ("Opacity At End", fade.w, 0, 1);

                     if (EditorGUI.EndChangeCheck ())
                     {
                        mat.SetVector ("_GlobalNormalFade", fade);
                        EditorUtility.SetDirty (mat);
                     }
                  }
                  if (mat.HasProperty ("_GlobalNormalUVScale"))
                  {
                     Vector4 uv = mat.GetVector ("_GlobalNormalUVScale");
                     Vector2 scale = new Vector2 (uv.x, uv.y);
                     Vector2 offset = new Vector2 (uv.z, uv.w);

                     EditorGUI.BeginChangeCheck ();
                     scale = EditorGUILayout.Vector2Field (CUVScale, scale);
                     offset = EditorGUILayout.Vector2Field (CUVOffset, offset);

                     if (EditorGUI.EndChangeCheck ())
                     {
                        uv = new Vector4 (scale.x, scale.y, offset.x, offset.y);
                        mat.SetVector ("_GlobalNormalUVScale", uv);
                        EditorUtility.SetDirty (mat);
                     }
                  }
               }
            }
            // saom
            if (SAOMBlend != SAOMBlendMode.Off && mat.HasProperty("_GlobalSAOMTex") && normalPackMode != NormalPackMode.Packed)
            {
               using (new GUILayout.VerticalScope (GUI.skin.box))
               {
                  materialEditor.TexturePropertySingleLine (new GUIContent ("Smoothness(R)/AO(G)/Metal(B) Texture", "Global smoothness, ao, metallic Texture"), shaderGUI.FindProp ("_GlobalSAOMTex", props));
                  Vector4 parms = mat.GetVector ("_GlobalTextureParams");
                  EditorGUI.BeginChangeCheck ();
                  parms.z = EditorGUILayout.Slider ("Blend", parms.z, 0, 3);
                  if (EditorGUI.EndChangeCheck ())
                  {
                     mat.SetVector ("_GlobalTextureParams", parms);
                     EditorUtility.SetDirty (mat);
                  }

                  if (mat.HasProperty ("_GlobalSAOMFade"))
                  {
                     Vector4 fade = mat.GetVector ("_GlobalSAOMFade");
                     EditorGUI.BeginChangeCheck ();

                     fade.x = EditorGUILayout.FloatField ("Begin Fade", fade.x);
                     fade.z = EditorGUILayout.Slider ("Opacity At Begin", fade.z, 0, 1);
                     fade.y = EditorGUILayout.FloatField ("Fade Range", fade.y);
                     fade.w = EditorGUILayout.Slider ("Opacity At End", fade.w, 0, 1);

                     if (EditorGUI.EndChangeCheck ())
                     {
                        mat.SetVector ("_GlobalSAOMFade", fade);
                        EditorUtility.SetDirty (mat);
                     }
                  }
                  if (mat.HasProperty ("_GlobalSAOMUVScale"))
                  {
                     Vector4 uv = mat.GetVector ("_GlobalSAOMUVScale");
                     Vector2 scale = new Vector2 (uv.x, uv.y);
                     Vector2 offset = new Vector2 (uv.z, uv.w);

                     EditorGUI.BeginChangeCheck ();
                     scale = EditorGUILayout.Vector2Field (CUVScale, scale);
                     offset = EditorGUILayout.Vector2Field (CUVOffset, offset);

                     if (EditorGUI.EndChangeCheck ())
                     {
                        uv = new Vector4 (scale.x, scale.y, offset.x, offset.y);
                        mat.SetVector ("_GlobalSAOMUVScale", uv);
                        EditorUtility.SetDirty (mat);
                     }
                  }
               }
            }

            if (specularBlendMode != BlendMode.Off && mat.HasProperty ("_GlobalSpecularTex"))
            {
               using (new GUILayout.VerticalScope (GUI.skin.box))
               {
                  materialEditor.TexturePropertySingleLine (new GUIContent ("Specular Texture", "Specular Tint Texture"), shaderGUI.FindProp ("_GlobalSpecularTex", props));
                  float parms = mat.GetFloat ("_GlobalSpecularBlend");
                  EditorGUI.BeginChangeCheck ();
                  parms = EditorGUILayout.Slider ("Blend", parms, 0, 1);
                  if (EditorGUI.EndChangeCheck ())
                  {
                     mat.SetFloat ("_GlobalSpecularBlend", parms);
                     EditorUtility.SetDirty (mat);
                  }
                  if (mat.HasProperty ("_GlobalSpecularFade"))
                  {
                     Vector4 fade = mat.GetVector ("_GlobalSpecularFade");
                     EditorGUI.BeginChangeCheck ();

                     fade.x = EditorGUILayout.FloatField ("Begin Fade", fade.x);
                     fade.z = EditorGUILayout.Slider ("Opacity At Begin", fade.z, 0, 1);
                     fade.y = EditorGUILayout.FloatField ("Fade Range", fade.y);
                     fade.w = EditorGUILayout.Slider ("Opacity At End", fade.w, 0, 1);

                     if (EditorGUI.EndChangeCheck ())
                     {
                        mat.SetVector ("_GlobalSpecularFade", fade);
                        EditorUtility.SetDirty (mat);
                     }
                  }
                  if (mat.HasProperty ("_GlobalSpecularUVScale"))
                  {
                     Vector4 uv = mat.GetVector ("_GlobalSpecularUVScale");
                     Vector2 scale = new Vector2 (uv.x, uv.y);
                     Vector2 offset = new Vector2 (uv.z, uv.w);

                     EditorGUI.BeginChangeCheck ();
                     scale = EditorGUILayout.Vector2Field (CUVScale, scale);
                     offset = EditorGUILayout.Vector2Field (CUVOffset, offset);

                     if (EditorGUI.EndChangeCheck ())
                     {
                        uv = new Vector4 (scale.x, scale.y, offset.x, offset.y);
                        mat.SetVector ("_GlobalSpecularUVScale", uv);
                        EditorUtility.SetDirty (mat);
                     }
                  }
               }
            }

            // emis
            if (emisBlend != SAOMBlendMode.Off && mat.HasProperty("_GlobalEmisTex"))
            {
               using (new GUILayout.VerticalScope (GUI.skin.box))
               {
                  materialEditor.TexturePropertySingleLine (new GUIContent ("Emission Texture", "Global Emission"), shaderGUI.FindProp ("_GlobalEmisTex", props));
                  Vector4 parms = mat.GetVector ("_GlobalTextureParams");
                  EditorGUI.BeginChangeCheck ();
                  parms.w = EditorGUILayout.Slider ("Blend", parms.w, 0, 3);
                  if (EditorGUI.EndChangeCheck ())
                  {
                     mat.SetVector ("_GlobalTextureParams", parms);
                     EditorUtility.SetDirty (mat);
                  }

                  if (mat.HasProperty ("_GlobalEmisFade"))
                  {
                     Vector4 fade = mat.GetVector ("_GlobalEmisFade");
                     EditorGUI.BeginChangeCheck ();

                     fade.x = EditorGUILayout.FloatField ("Begin Fade", fade.x);
                     fade.z = EditorGUILayout.Slider ("Opacity At Begin", fade.z, 0, 1);
                     fade.y = EditorGUILayout.FloatField ("Fade Range", fade.y);
                     fade.w = EditorGUILayout.Slider ("Opacity At End", fade.w, 0, 1);

                     if (EditorGUI.EndChangeCheck ())
                     {
                        mat.SetVector ("_GlobalEmisFade", fade);
                        EditorUtility.SetDirty (mat);
                     }
                  }
                  if (mat.HasProperty ("_GlobalEmisUVScale"))
                  {
                     Vector4 uv = mat.GetVector ("_GlobalEmisUVScale");
                     Vector2 scale = new Vector2 (uv.x, uv.y);
                     Vector2 offset = new Vector2 (uv.z, uv.w);

                     EditorGUI.BeginChangeCheck ();
                     scale = EditorGUILayout.Vector2Field (CUVScale, scale);
                     offset = EditorGUILayout.Vector2Field (CUVOffset, offset);

                     if (EditorGUI.EndChangeCheck ())
                     {
                        uv = new Vector4 (scale.x, scale.y, offset.x, offset.y);
                        mat.SetVector ("_GlobalEmisUVScale", uv);
                        EditorUtility.SetDirty (mat);
                     }
                  }
               }
            }
            if (globalSlopeFilter && mat.HasProperty ("_GlobalSlopeTex"))
            {

               var propData = MicroSplatShaderGUI.FindOrCreatePropTex(mat);
               EditorGUI.BeginChangeCheck();
               if (propData != null)
               {
                  EditorGUILayout.BeginHorizontal ();
                  propData.globalSlopeFilter = EditorGUILayout.CurveField(CGeoSlope, propData.globalSlopeFilter, Color.white, new Rect(0,0,1,1));
                  if (GUILayout.Button ("P", GUILayout.Width (22)))
                  {
                     InitPresets ();
                     GenericMenu menu = new GenericMenu ();
                     menu.AddItem(new GUIContent(PresetNames.Floor.ToString()), currentPreset == PresetNames.Floor, OnGlobalPresetSelected, new PresetSelection(PresetNames.Floor, propData));
                     menu.AddItem(new GUIContent(PresetNames.Slopes.ToString()), currentPreset == PresetNames.Slopes, OnGlobalPresetSelected, new PresetSelection(PresetNames.Slopes, propData));
                     menu.AddItem(new GUIContent(PresetNames.Cliff.ToString()), currentPreset == PresetNames.Cliff, OnGlobalPresetSelected, new PresetSelection(PresetNames.Cliff, propData));
                     menu.ShowAsContext();
                  }
                  EditorGUILayout.EndHorizontal ();
               }

               if (EditorGUI.EndChangeCheck())
               {
                  EditorUtility.SetDirty(mat);
                  EditorUtility.SetDirty(propData);
                  MicroSplatObject.SyncAll();
               }
            }
         }
      }

      static GUIContent CPerTexGeo = new GUIContent("Geo Strength", "How much the geo texture should show on this texture");
      static GUIContent CPerTexGeoHeight = new GUIContent("Geo Height Filter", "Filter Geo Strength by texture height map, allowing height or low areas to be filtered out. Negative values invert the filter.");
      static GUIContent CPerTexGeoHeightStrength = new GUIContent("Geo Height Filter Strength", "how much filtered areas should be tinted");
      static GUIContent CPerTexTint = new GUIContent("Global Tint Strength", "How much the global tint texture should show on this texture");
      static GUIContent CPerTexNormal = new GUIContent("Global Normal Strength", "How much the global normal texture should show on this texture");
      static GUIContent CPerTexSAOM = new GUIContent("Global Smoothness/AO/Metal Strength", "How much the global smoothness/ao/metal texture should show on this texture");
      static GUIContent CPerTexEmis = new GUIContent("Global Emissive Strength", "How much the global emission texture should show on this texture");
      static GUIContent CPerTexSpecular = new GUIContent ("Global Specular Strength", "How much the global specular texture should show on this texture");

      public override void DrawPerTextureGUI(int index, MicroSplatKeywords keywords, Material mat, MicroSplatPropData propData)
      {
         InitPropData(5, propData, new Color(1.0f, 1.0f, 1.0f, 1.0f)); //geoTexture, global tint, global normal
         if (geoTexture != GeoTextureMode.Off)
         {
            perTexGeoStr = DrawPerTexFloatSlider(index, 5, GetFeatureName(DefineFeature._PERTEXGEO),
               keywords, propData, Channel.R,  CPerTexGeo, 0, 1);
            perTexGeoHeight = DrawPerTexFloatSlider(index, 8, GetFeatureName(DefineFeature._PERTEXGEOMAPHEIGHT),
               keywords, propData, Channel.B, CPerTexGeoHeight, -1, 1);
            perTexGeoHeightStrength = DrawPerTexFloatSlider(index, 8, GetFeatureName(DefineFeature._PERTEXGEOMAPHEIGHTSTRENGTH),
               keywords, propData, Channel.A, CPerTexGeoHeightStrength, 0, 1);
         }
         if (tintBlendMode != BlendMode.Off)
         {
            perTexTintStr = DrawPerTexFloatSlider(index, 5, GetFeatureName(DefineFeature._PERTEXGLOBALTINTSTRENGTH),
               keywords, propData, Channel.G,  CPerTexTint, 0, 1);
         }
         if (normalBlendMode != NormalBlendMode.Off)
         {
            perTexNormalStr = DrawPerTexFloatSlider(index, 5, GetFeatureName(DefineFeature._PERTEXGLOBALNORMALSTRENGTH),
               keywords, propData, Channel.B,  CPerTexNormal, 0, 2);
         }
         if (SAOMBlend != SAOMBlendMode.Off)
         {
            perTexSAOMStr = DrawPerTexFloatSlider(index, 5, GetFeatureName(DefineFeature._PERTEXGLOBALSOAMSTRENGTH),
               keywords, propData, Channel.A, CPerTexSAOM, 0, 2);
         }
         if (specularBlendMode != BlendMode.Off)
         {
            perTexSpecularStr = DrawPerTexFloatSlider (index, 16, GetFeatureName (DefineFeature._PERTEXGLOBALSPECULARSTRENGTH),
               keywords, propData, Channel.A, CPerTexSpecular, 0, 1);
         }
         if (emisBlend != SAOMBlendMode.Off)
         {
            perTexEmisStr = DrawPerTexFloatSlider(index, 6, GetFeatureName(DefineFeature._PERTEXGLOBALEMISSTRENGTH),
               keywords, propData, Channel.A, CPerTexEmis, 0, 2);
         }
      }

      public override void InitCompiler(string[] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths[i];
            if (p.EndsWith("microsplat_properties_geomap.txt"))
            {
               properties_geomap = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_func_geomap.txt"))
            {
               function_geomap = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith ("microsplat_cbuffer_geomap.txt"))
            {
               cbuffer_geomap = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
            if (p.EndsWith("microsplat_properties_globalnormal.txt"))
            {
               properties_normal = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_properties_globaltint.txt"))
            {
               properties_tint = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_properties_globalsaom.txt"))
            {
               properties_saom = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith ("microsplat_properties_globalspecular.txt"))
            {
               properties_specular = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
            if (p.EndsWith("microsplat_properties_globalemis.txt"))
            {
               properties_emis = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith("microsplat_properties_globalparams.txt"))
            {
               properties_params = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
             
         }
      } 

      public override void WriteProperties(string[] features, System.Text.StringBuilder sb)
      {
         if (geoTexture != GeoTextureMode.Off)
         {
            sb.Append(properties_geomap.text);
         }
         if (tintBlendMode != BlendMode.Off)
         {
            sb.Append(properties_tint.text);
         }
         if (normalBlendMode != NormalBlendMode.Off)
         {
            sb.Append(properties_normal.text);
         }
         if (SAOMBlend != SAOMBlendMode.Off)
         {
            sb.Append(properties_saom.text);
         }
         if (specularBlendMode != BlendMode.Off)
         {
            sb.AppendLine(properties_specular.text);
         }
         if (emisBlend != SAOMBlendMode.Off)
         {
            sb.Append(properties_emis.text);
         }

         if (tintBlendMode != BlendMode.Off || normalBlendMode != NormalBlendMode.Off || emisBlend != SAOMBlendMode.Off || SAOMBlend != SAOMBlendMode.Off || specularBlendMode != BlendMode.Off)
         {
            sb.Append(properties_params.text);
            if (globalSlopeFilter)
            {
               sb.AppendLine ("         _GlobalSlopeTex(\"GlobalSlopeTex\", 2D) = \"white\" {}");
            }
         }

         if (splatFade)
         {
            sb.AppendLine ("      _SplatFade(\"Splat Fade\", Vector) = (200, 400, 0, 0)");
         }

      }

      public override void ComputeSampleCounts(string[] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {
         if (geoTexture != GeoTextureMode.Off)
         {
            textureSampleCount++;
            if (geoCurve)
            {
               textureSampleCount++;
            }
            if (geoTexture == GeoTextureMode.AlbedoNormal)
            {
               textureSampleCount++;
            }
            if (geoSlopeFilter)
            {
               textureSampleCount++;
            }
         }
         if (tintBlendMode != BlendMode.Off)
         {
            textureSampleCount++;
            if (tintSampleMode == SamplingMode.Quadratic)
            {
               textureSampleCount += 3;
            }
         }
         if (normalBlendMode != NormalBlendMode.Off)
         {
            textureSampleCount++;
            if (normalSampleMode == SamplingMode.Quadratic)
            {
               textureSampleCount += 3;
            }
         }
         if (emisBlend != SAOMBlendMode.Off)
         {
            textureSampleCount++;
            if (emisSampleMode == SamplingMode.Quadratic)
            {
               textureSampleCount += 3;
            }
         }
         if (SAOMBlend != SAOMBlendMode.Off)
         {
            textureSampleCount++;
            if (saomSampleMode == SamplingMode.Quadratic)
            {
               textureSampleCount += 3;
            }
         }
         if (globalSlopeFilter)
         {
            textureSampleCount++;
         }
         if (specularBlendMode != BlendMode.Off)
         {
            textureSampleCount++;
            if (specularSampleMode == SamplingMode.Quadratic)
            {
               textureSampleCount += 3;
            }
         }
      }

      public override string[] Pack()
      {
         List<string> features = new List<string>();
         if (geoTexture != GeoTextureMode.Off)
         {
            features.Add(GetFeatureName(DefineFeature._GEOMAP));
            if (geoTexture == GeoTextureMode.AlbedoNormal)
            {
               features.Add(GetFeatureName(DefineFeature._GEONORMAL));
            }
            if (geoTextureBlendMode == GeoTextureBlendMode.LightColor)
            {
               features.Add (GetFeatureName (DefineFeature._GEOTEXLIGHTCOLOR));
            }
            if (perTexGeoStr)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXGEO));
            }
            if (perTexGeoHeight)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXGEOMAPHEIGHT));
               if (perTexGeoHeightStrength)
               {
                  features.Add(GetFeatureName(DefineFeature._PERTEXGEOMAPHEIGHTSTRENGTH));
               }
            }
            if (geoRange)
            {
               features.Add(GetFeatureName(DefineFeature._GEORANGE));
            }
            if (geoCurve)
            {
               features.Add(GetFeatureName(DefineFeature._GEOCURVE));
            }
            if (geoSlopeFilter)
            {
               features.Add (GetFeatureName (DefineFeature._GEOSLOPEFILTER));
            }
         }

         if (splatFade)
         {
            features.Add (GetFeatureName (DefineFeature._SPLATFADE));
         }

         if (tintBlendMode != BlendMode.Off)
         {
            features.Add(GetFeatureName(DefineFeature._GLOBALTINT));
            if (perTexTintStr)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXGLOBALTINTSTRENGTH));
            }
            if (tintBlendMode == BlendMode.Multiply2X)
            {
               features.Add(GetFeatureName(DefineFeature._GLOBALTINTMULT2X));
            }
            else if (tintBlendMode == BlendMode.Overlay)
            {
               features.Add(GetFeatureName(DefineFeature._GLOBALTINTOVERLAY));
            }
            else if (tintBlendMode == BlendMode.LightColor)
            {
               features.Add (GetFeatureName (DefineFeature._GLOBALTINTLIGHTCOLOR));
            }
            else
            {
               features.Add(GetFeatureName(DefineFeature._GLOBALTINTCROSSFADE));
            }
            if (tintSampleMode == SamplingMode.Quadratic)
            {
               features.Add (GetFeatureName (DefineFeature._GLOBALTINTBIQUADRATIC));
            }
         }
         if (normalBlendMode != NormalBlendMode.Off)
         {
            features.Add(GetFeatureName(DefineFeature._GLOBALNORMALS));
            if (normalPackMode == NormalPackMode.Packed)
            {
               features.Add(GetFeatureName(DefineFeature._GLOBALNORMALPACKEDSAO));
            }
            if (perTexNormalStr)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXGLOBALNORMALSTRENGTH));
            }
            if (normalBlendMode == NormalBlendMode.CrossFade)
            {
               features.Add(GetFeatureName(DefineFeature._GLOBALNORMALCROSSFADE));
            }
            if (normalSampleMode == SamplingMode.Quadratic)
            {
               features.Add (GetFeatureName (DefineFeature._GLOBALNORMALSBIQUADRATIC));
            }
         }
         if (specularBlendMode != BlendMode.Off)
         {
            features.Add (GetFeatureName (DefineFeature._GLOBALSPECULAR));
            if (perTexSpecularStr)
            {
               features.Add (GetFeatureName (DefineFeature._PERTEXGLOBALSPECULARSTRENGTH));
            }
            if (specularBlendMode == BlendMode.Multiply2X)
            {
               features.Add (GetFeatureName (DefineFeature._GLOBALSPECULARMULT2X));
            }
            else if (specularBlendMode == BlendMode.Overlay)
            {
               features.Add (GetFeatureName (DefineFeature._GLOBALSPECULAROVERLAY));
            }
            else if (specularBlendMode == BlendMode.LightColor)
            {
               features.Add (GetFeatureName (DefineFeature._GLOBALSPECULARLIGHTCOLOR));
            }
            else
            {
               features.Add (GetFeatureName (DefineFeature._GLOBALSPECULARCROSSFADE));
            }

            if (specularSampleMode == SamplingMode.Quadratic)
            {
               features.Add (GetFeatureName (DefineFeature._GLOBALSPECBIQUADRATIC));
            }
         }
         if (SAOMBlend != SAOMBlendMode.Off && normalPackMode != NormalPackMode.Packed)
         {
            features.Add(GetFeatureName(DefineFeature._GLOBALSMOOTHAOMETAL));
            if (perTexSAOMStr)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXGLOBALSOAMSTRENGTH));
            }
            if (saomSampleMode == SamplingMode.Quadratic)
            {
               features.Add (GetFeatureName (DefineFeature._GLOBALSAOMBIQUADRATIC));
            }
         }
         if (emisBlend != SAOMBlendMode.Off)
         {
            features.Add(GetFeatureName(DefineFeature._GLOBALEMIS));
            if (perTexEmisStr)
            {
               features.Add(GetFeatureName(DefineFeature._PERTEXGLOBALEMISSTRENGTH));
            }
            if (emisSampleMode == SamplingMode.Quadratic)
            {
               features.Add (GetFeatureName (DefineFeature._GLOBALEMISBIQUADRATIC));
            }
         }

         if (tintBlendMode != BlendMode.Off || normalBlendMode != NormalBlendMode.Off || SAOMBlend != SAOMBlendMode.Off || emisBlend != SAOMBlendMode.Off)
         {
            if (wrapMode == WrapMode.Wrap)
            {
               features.Add (GetFeatureName (DefineFeature._GLOBALTEXWRAP));
            }
            if (globalSlopeFilter)
            {
               features.Add (GetFeatureName (DefineFeature._GLOBALSLOPEFILTER));
            }
            if (globalNoiseUV)
            {
               features.Add (GetFeatureName (DefineFeature._GLOBALNOISEUV));
            }
         }
         return features.ToArray();
      }

      public override void WritePerMaterialCBuffer (string[] features, System.Text.StringBuilder sb)
      {
         if (geoTexture != GeoTextureMode.Off || tintBlendMode != BlendMode.Off || normalBlendMode != NormalBlendMode.Off ||
            SAOMBlend != SAOMBlendMode.Off || emisBlend != SAOMBlendMode.Off || specularBlendMode != BlendMode.Off)
         {
            sb.AppendLine(cbuffer_geomap.text);
         }
         if (splatFade)
         {
            sb.AppendLine ("      float4 _SplatFade;");
         }
      }

      public override void WriteFunctions(string [] features, System.Text.StringBuilder sb)
      {
         if (geoTexture != GeoTextureMode.Off || tintBlendMode != BlendMode.Off || normalBlendMode != NormalBlendMode.Off ||
            SAOMBlend != SAOMBlendMode.Off || emisBlend != SAOMBlendMode.Off || specularBlendMode != BlendMode.Off)
         {
            sb.AppendLine(function_geomap.text);
         }

      }

      public override void Unpack(string[] keywords)
      {
         geoTexture = GeoTextureMode.Off;
         if (HasFeature(keywords, DefineFeature._GEOMAP))
         {
            geoTexture = GeoTextureMode.Albedo;
            if (HasFeature(keywords, DefineFeature._GEONORMAL))
            {
               geoTexture = GeoTextureMode.AlbedoNormal;
            }
            geoTextureBlendMode = GeoTextureBlendMode.Multiply2X;
            if (HasFeature(keywords, DefineFeature._GEOTEXLIGHTCOLOR))
            {
               geoTextureBlendMode = GeoTextureBlendMode.LightColor;
            }
         }


         if (geoTexture != GeoTextureMode.Off)
         {
            perTexGeoStr = HasFeature(keywords, DefineFeature._PERTEXGEO);
            geoRange = HasFeature(keywords, DefineFeature._GEORANGE);
            geoCurve = HasFeature(keywords, DefineFeature._GEOCURVE);
            geoSlopeFilter = HasFeature(keywords, DefineFeature._GEOSLOPEFILTER);
            
            perTexGeoHeight = HasFeature(keywords, DefineFeature._PERTEXGEOMAPHEIGHT);
            perTexGeoHeightStrength = false;
            if (perTexGeoHeight)
            { 
               perTexGeoHeightStrength = HasFeature(keywords, DefineFeature._PERTEXGEOMAPHEIGHTSTRENGTH);
            }
         }
            
         perTexTintStr = HasFeature(keywords, DefineFeature._PERTEXGLOBALTINTSTRENGTH);
         splatFade = (HasFeature (keywords, DefineFeature._SPLATFADE));

         tintBlendMode = BlendMode.Off;
         if (HasFeature(keywords, DefineFeature._GLOBALTINT))
         {
            tintBlendMode = BlendMode.Multiply2X;
         }
         if (HasFeature(keywords, DefineFeature._GLOBALTINTOVERLAY))
         {
            tintBlendMode = BlendMode.Overlay;
         }
         if (HasFeature(keywords, DefineFeature._GLOBALTINTLIGHTCOLOR))
         {
            tintBlendMode = BlendMode.LightColor;
         }
         if (HasFeature(keywords, DefineFeature._GLOBALTINTCROSSFADE))
         {
            tintBlendMode = BlendMode.CrossFade;
         }
         tintSampleMode = HasFeature (keywords, DefineFeature._GLOBALTINTBIQUADRATIC) ? SamplingMode.Quadratic : SamplingMode.Linear;

         normalBlendMode = NormalBlendMode.Off;
         if (HasFeature(keywords, DefineFeature._GLOBALNORMALS))
         {
            normalBlendMode = NormalBlendMode.NormalBlend;
         }
         if (HasFeature(keywords, DefineFeature._GLOBALNORMALCROSSFADE))
         {
            normalBlendMode = NormalBlendMode.CrossFade;
         }
         normalSampleMode = HasFeature (keywords, DefineFeature._GLOBALNORMALSBIQUADRATIC) ? SamplingMode.Quadratic : SamplingMode.Linear;

         normalPackMode = HasFeature(keywords, DefineFeature._GLOBALNORMALPACKEDSAO) ? NormalPackMode.Packed : NormalPackMode.Separate;

         SAOMBlend = HasFeature(keywords, DefineFeature._GLOBALSMOOTHAOMETAL) ? SAOMBlendMode.CrossFade : SAOMBlendMode.Off;
         saomSampleMode = HasFeature (keywords, DefineFeature._GLOBALSAOMBIQUADRATIC) ? SamplingMode.Quadratic : SamplingMode.Linear;

         specularBlendMode = BlendMode.Off;
         if (HasFeature (keywords, DefineFeature._GLOBALSPECULAR))
         {
            specularBlendMode = BlendMode.Multiply2X;
         }
         if (HasFeature (keywords, DefineFeature._GLOBALSPECULAROVERLAY))
         {
            specularBlendMode = BlendMode.Overlay;
         }
         if (HasFeature (keywords, DefineFeature._GLOBALSPECULARCROSSFADE))
         {
            specularBlendMode = BlendMode.CrossFade;
         }
         if (HasFeature(keywords, DefineFeature._GLOBALSPECULARLIGHTCOLOR))
         {
            specularBlendMode = BlendMode.LightColor;
         }
         specularSampleMode = HasFeature (keywords, DefineFeature._GLOBALSPECBIQUADRATIC) ? SamplingMode.Quadratic : SamplingMode.Linear;

         emisBlend = HasFeature(keywords, DefineFeature._GLOBALEMIS) ? SAOMBlendMode.CrossFade : SAOMBlendMode.Off;
         emisSampleMode = HasFeature (keywords, DefineFeature._GLOBALEMISBIQUADRATIC) ? SamplingMode.Quadratic : SamplingMode.Linear;
         wrapMode = HasFeature (keywords, DefineFeature._GLOBALTEXWRAP) ? WrapMode.Wrap : WrapMode.Clamp;
         globalNoiseUV = HasFeature (keywords, DefineFeature._GLOBALNOISEUV);
         globalSlopeFilter = HasFeature (keywords, DefineFeature._GLOBALSLOPEFILTER);
         perTexNormalStr = HasFeature(keywords, DefineFeature._PERTEXGLOBALNORMALSTRENGTH);
         perTexSAOMStr = HasFeature(keywords, DefineFeature._PERTEXGLOBALSOAMSTRENGTH);
         perTexEmisStr = HasFeature(keywords, DefineFeature._PERTEXGLOBALEMISSTRENGTH);
         perTexSpecularStr = HasFeature (keywords, DefineFeature._PERTEXGLOBALSPECULARSTRENGTH);
      }

   }   
   #endif


}
