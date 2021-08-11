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
   public class MicroSplatProceduralTexture : FeatureDescriptor
   {
      const string sGlobalTextureDefine = "__MICROSPLAT_PROCTEX__";
      static MicroSplatProceduralTexture ()
      {
         MicroSplatDefines.InitDefine (sGlobalTextureDefine);
      }
      [PostProcessSceneAttribute (0)]
      public static void OnPostprocessScene ()
      {
         MicroSplatDefines.InitDefine (sGlobalTextureDefine);
      }

      public override string ModuleName ()
      {
         return "Procedural Texturing";
      }

      public override string GetHelpPath ()
      {
         return "/MicroSplat/Core/Documentation/MicroSplat - Procedural Texturing.pdf";
      }

      public enum DefineFeature
      {
         _PROCEDURALTEXTURE,
         _PROCEDURALBLENDSPLATS,
         _PCBIOMEMASK,
         _PCBIOMEMASK2,
         _PCBIOMEMASK16,
         _PCNOISEUV,
         _PCNOISETRIPLANAR,
         _PCNOISEPROCEDURAL,
         _PCHEIGHTGRADIENT,
         _PCHEIGHTHSV,
         _PCSLOPEGRADIENT,
         _PCSLOPEHSV,
         _PCCAVITY,
         _PCFLOW,
         _PCUSECOMBINEDNORMAL, // slope
         _PCUSECOMBINEDHEIGHT, // height
         _PCUSECOMBINEDAO,     // cavity
         kNumFeatures,
      }

      public enum NoiseType
      {
         Texture,
         Procedural
      }

      public enum NoiseSpace
      {
         UV,
         World,
         Triplanar
      }

      public enum BiomeMaskType
      {
         None,
         SplatMap,
         SplatMap2,
         ExclusionMap,
      }

      public enum PresetNames
      {
         Floor,
         Slopes,
         Cliff
      }

      public enum CombinedNormalSource
      {
         VertexNormal,
         CombinedNormal
      }

      public enum CombinedHeightSource
      {
         WorldHeight,
         CombinedHeightMap
      }

      public enum CombinedCavitySource
      {
         CavityMap,
         CombinedAOChannel
      }

      public bool combineWithSplat;

      static AnimationCurve [] slopePresets;



      public bool proceduralTexture;
      public BiomeMaskType biomeMask = BiomeMaskType.None;
      public NoiseSpace noiseSpace = NoiseSpace.World;
      public bool heightGradients;
      public bool heightHSV;
      public bool slopeGradients;
      public bool slopeHSV;
      public bool cavityMap, erosionMap;
      public NoiseType noiseType = NoiseType.Texture;

      public TextAsset properties_texturing;
      public TextAsset function_texturing;
      public TextAsset function_gradient;
      public TextAsset cbuffer_texturing;
      public TextAsset cbuffer_gradient;

      public CombinedNormalSource normalSource = CombinedNormalSource.VertexNormal;
      public CombinedHeightSource heightSource = CombinedHeightSource.WorldHeight;
      public CombinedCavitySource cavitySource = CombinedCavitySource.CavityMap;

      GUIContent CCombineWithSplat = new GUIContent ("Combine With Splat", "When enabled, splat textures are sampled and a single texture index is used to decide where procedural texturing is used. This allows you to paint the other textures where ever you want them");
      GUIContent CUseCombinedNormal = new GUIContent ("Slope Source", "Use the normal map from the combined shader for slope");
      GUIContent CUseCombinedHeight = new GUIContent ("Height Source", "Use height map from base shader for height (0-1)");
      GUIContent CUseCombinedAO = new GUIContent ("Cavity Source", "Use the AO from the base shader for cavity");

      GUIContent CProceduralTexture = new GUIContent ("Procedural Texture", "Enable run-time procedural texturing");
      GUIContent CBiomeMaskOption = new GUIContent ("Use Biome Mask", "When set to SplatMap, an RGBA weight texture can be used to control biome weights. When set to ExclusionMap, 16 regions can be filtered by using colors like 1,0,1,0 and such");
      GUIContent CNoiseSpace = new GUIContent ("Noise Space", "UV space to use for noise lookups. Triplanar is more expensive");
      GUIContent CNoiseType = new GUIContent ("Noise Type", "Texture noise can be slightly faster and uses one channel of the texture for each filter type (height, slope, etc). Procedural noise is generated once for all four channels and is more expensive, but has no repeating pattenrs");
      GUIContent CCavityMap = new GUIContent ("Cavity Map", "Enable filtering based on cavity, which must be precalculated into a texture");
      GUIContent CErosionMap = new GUIContent ("Erosion Map", "Enable filtering based on erosion, which must be precalculated into a texture");

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

      void InitPresets ()
      {
         if (slopePresets == null)
         {
            slopePresets = new AnimationCurve [3];
            slopePresets [0] = AnimationCurve.EaseInOut (0.0f, 1.0f, 0.15f, 0);
            slopePresets [1] = new AnimationCurve (new Keyframe [4] { new Keyframe (0.03f, 0), new Keyframe (0.06f, 1), new Keyframe (0.16f, 1), new Keyframe (0.2f, 0) });
            slopePresets [2] = new AnimationCurve (new Keyframe [2] { new Keyframe (0.18f, 0), new Keyframe (0.35f, 1) });
         }
      }


      public override void DrawFeatureGUI (MicroSplatKeywords keywords)
      {
         if (keywords.IsKeywordEnabled ("_DISABLESPLATMAPS"))
            return;
         proceduralTexture = EditorGUILayout.Toggle (CProceduralTexture, proceduralTexture);
         if (proceduralTexture)
         {
            EditorGUI.indentLevel++;
            combineWithSplat = EditorGUILayout.Toggle (CCombineWithSplat, combineWithSplat);
            biomeMask = (BiomeMaskType)EditorGUILayout.EnumPopup (CBiomeMaskOption, biomeMask);
            noiseType = (NoiseType)EditorGUILayout.EnumPopup (CNoiseType, noiseType);
            if (noiseType == NoiseType.Texture)
            {
               noiseSpace = (NoiseSpace)EditorGUILayout.EnumPopup (CNoiseSpace, noiseSpace);
            }
            bool oldcav = cavityMap;
            cavityMap = EditorGUILayout.Toggle (CCavityMap, cavityMap);
            if (oldcav != cavityMap && cavityMap)
            {
               cavitySource = CombinedCavitySource.CavityMap;
            }
            erosionMap = EditorGUILayout.Toggle (CErosionMap, erosionMap);

            if (keywords.IsKeywordEnabled ("_MICROMESH") && keywords.IsKeywordEnabled ("_MESHCOMBINED"))
            {
               normalSource = (CombinedNormalSource)EditorGUILayout.EnumPopup (CUseCombinedNormal, normalSource);
               heightSource = (CombinedHeightSource)EditorGUILayout.EnumPopup (CUseCombinedHeight, heightSource);
               var old = cavitySource;
               cavitySource = (CombinedCavitySource)EditorGUILayout.EnumPopup (CUseCombinedAO, cavitySource);
               if (cavitySource == CombinedCavitySource.CombinedAOChannel && old != cavitySource)
               {
                  cavityMap = false;
               }
            }
            EditorGUI.indentLevel--;
         }
      }

      static MicroSplatProceduralTextureConfig Find (Material mat)
      {
         if (mat == null)
            return null;
         var path = AssetDatabase.GetAssetPath (mat);
         path = path.Replace ("\\", "/");
         path = path.Replace (".mat", "_proceduraltexture.asset");
         // mesh terrains are in a sub directory when lod'd, so seak back and get the shared propData
         if (path.Contains ("MeshTerrain/MicroSplatData/") && !System.IO.File.Exists (path))
         {
            path = path.Replace ("MeshTerrain/MicroSplatData/", "");
         }

         MicroSplatProceduralTextureConfig cfg = AssetDatabase.LoadAssetAtPath<MicroSplatProceduralTextureConfig> (path);
         return cfg;
      }

      public static MicroSplatProceduralTextureConfig FindOrCreateProceduralConfig (Material mat)
      {
         if (mat == null)
            return null;
         var path = AssetDatabase.GetAssetPath (mat);
         path = path.Replace ("\\", "/");
         path = path.Replace (".mat", "_proceduraltexture.asset");
         // mesh terrains are in a sub directory when lod'd, so seak back and get the shared propData
         if (path.Contains ("MeshTerrain/MicroSplatData/") && !System.IO.File.Exists (path))
         {
            path = path.Replace ("MeshTerrain/MicroSplatData/", "");
         }

         MicroSplatProceduralTextureConfig cfg = AssetDatabase.LoadAssetAtPath<MicroSplatProceduralTextureConfig> (path);
         if (cfg == null)
         {
            cfg = ScriptableObject.CreateInstance<MicroSplatProceduralTextureConfig> ();
            cfg.ResetToDefault ();
            AssetDatabase.CreateAsset (cfg, path);
            UnityEditor.AssetDatabase.SaveAssets ();

            // this has to be wrapped, so core will compile first, then this will compile and install preprocessor macros,
            // then this will compile.
#if __MICROSPLAT_PROCTEX__
            // go through all MSO's and see if they grab this object and then sync them
            var msos = GameObject.FindObjectsOfType<MicroSplatObject> ();
            bool sync = false;
            foreach (var mso in msos)
            {
               if (mso.procTexCfg == null && mso.templateMaterial != null)
               {
                  var nc = Find (mso.templateMaterial);
                  if (nc == cfg)
                  {
                     mso.procTexCfg = cfg;
                     EditorUtility.SetDirty (mso.gameObject);
                     sync = true;
                  }
               }
            }
            if (sync)
            {
               MicroSplatObject.SyncAll ();
            }
#endif
         }

         return cfg;

      }

      void DrawArrayHeader (MicroSplatProceduralTextureConfig cfg)
      {
         int count = EditorGUILayout.IntField ("Layer Count", cfg.layers.Count);

         int num = count - cfg.layers.Count;
         while (num > 0)
         {
            num--;
            cfg.layers.Add (new MicroSplatProceduralTextureConfig.Layer ());
         }
         while (num < 0)
         {
            num++;
            cfg.layers.RemoveAt (cfg.layers.Count - 1);
         }

      }

      static GUIContent CNoiseTex = new GUIContent ("Noise Texture", "Noise texture used for all noise parameters. R channel affects height, G channel affects slope");
      static GUIContent CHeightRange = new GUIContent ("World Height Range", "Height curves are eventuated based on world position, so the range of the height values present is needed to map the curve");
      static GUIContent CBiomeMask = new GUIContent ("Biome Mask", "Biome mask which defines biomes weights in an RGBA texture.");
      static GUIContent CBiomeMask2 = new GUIContent ("Biome Mask 2", "A second biome mask which defines biomes weights in an RGBA texture for another 4 biomes.");
      static GUIContent CBiomeCurveWeight = new GUIContent ("Biome Curve Weight", "Changes the blending on Biomes from blurry to tightly defined borders");




      void DrawCurveDataGUI (ref AnimationCurve curve, ref MicroSplatProceduralTextureConfig.ValueMode valueMode, string xLabel)
      {
         EditorGUILayout.BeginHorizontal ();
         EditorGUILayout.LabelField (xLabel, GUILayout.Width (80));
         EditorGUILayout.LabelField ("Weight", GUILayout.Width (80));
         EditorGUILayout.LabelField ("In", GUILayout.Width (80));
         EditorGUILayout.LabelField ("Out", GUILayout.Width (80));
#if __MICROSPLAT_PLANET__
         valueMode = (MicroSplatProceduralTextureConfig.ValueMode)EditorGUILayout.EnumPopup (valueMode, GUILayout.Width (70));
#endif
         EditorGUILayout.EndHorizontal ();
         for (int i = 0; i < curve.length; ++i)
         {
            var key = curve.keys [i];
            EditorGUI.BeginChangeCheck ();
            EditorGUILayout.BeginHorizontal ();

            key.time = EditorGUILayout.FloatField (key.time, GUILayout.Width (80));
            if (valueMode == MicroSplatProceduralTextureConfig.ValueMode.Scalar)
            {
               key.value = EditorGUILayout.FloatField (key.value, GUILayout.Width (80));
            }
            else
            {
               int v = Mathf.RoundToInt (key.value * 255.0f);
               int newV = EditorGUILayout.IntField (v, GUILayout.Width (80));
               if (newV < 0)
                  newV = 0;
               if (newV > 255)
                  newV = 255;
               if (v != newV)
               {
                  key.value = (float)v / 255.0f;
               }
            }
            key.inTangent = EditorGUILayout.FloatField (key.inTangent, GUILayout.Width (80));
            key.outTangent = EditorGUILayout.FloatField (key.outTangent, GUILayout.Width (80));

            EditorGUILayout.EndHorizontal ();
            if (EditorGUI.EndChangeCheck ())
            {
               curve.keys [i] = key;
            }
         }
      }

      void CopyFilterToAnimCurve (MicroSplatProceduralTextureConfig.Layer.Filter f, AnimationCurve t, MicroSplatProceduralTextureConfig.Layer.CurveMode mode)
      {
         Keyframe [] keys = new Keyframe [3];

         keys [0] = new Keyframe (f.center - f.width, 0);
         keys [1] = new Keyframe (f.center, 1);
         keys [2] = new Keyframe (f.center + f.width, 0);
         t.keys = keys;

         // handle open ends
         if (mode == MicroSplatProceduralTextureConfig.Layer.CurveMode.LowPass)
         {
            keys [0].value = 1;
            keys [0].time = 0;
         }
         else if (mode == MicroSplatProceduralTextureConfig.Layer.CurveMode.HighPass)
         {
            keys [2].value = 1;
            keys [2].time = 1;
         }

         // handle inverse
         if (mode == MicroSplatProceduralTextureConfig.Layer.CurveMode.CutFilter)
         {
            keys [0].value = 1 - keys [0].value;
            keys [1].value = 1 - keys [1].value;
            keys [2].value = 1 - keys [2].value;
         }

         // clamp at value if out of range
         if (keys [0].time < 0)
         {
            keys [0].value = t.Evaluate (0);
            keys [0].time = 0;
         }
         if (keys [0].time > 1)
         {
            keys [0].value = t.Evaluate (1);
            keys [0].time = 1;
         }

         AnimationUtility.SetKeyLeftTangentMode (t, 0, AnimationUtility.TangentMode.Auto);
         AnimationUtility.SetKeyLeftTangentMode (t, 1, AnimationUtility.TangentMode.Auto);
         AnimationUtility.SetKeyLeftTangentMode (t, 2, AnimationUtility.TangentMode.Auto);
         AnimationUtility.SetKeyRightTangentMode (t, 0, AnimationUtility.TangentMode.Auto);
         AnimationUtility.SetKeyRightTangentMode (t, 1, AnimationUtility.TangentMode.Auto);
         AnimationUtility.SetKeyRightTangentMode (t, 2, AnimationUtility.TangentMode.Auto);
         t.keys = keys;
      }

      //static AnimationCurve scratchCurve = new AnimationCurve ();
      void DrawCurveToggle (ref bool toggle, ref MicroSplatProceduralTextureConfig.Layer.CurveMode curveMode, ref AnimationCurve curve,
         ref MicroSplatProceduralTextureConfig.Layer.Filter filter, string label, ref bool sheetToggle, bool endHorizontal = true)
      {
         EditorGUILayout.BeginHorizontal ();
         toggle = EditorGUILayout.Toggle (toggle, GUILayout.Width (40));
         EditorGUILayout.LabelField (label, GUILayout.Width (60));

         GUI.enabled = toggle;
         curveMode = (MicroSplatProceduralTextureConfig.Layer.CurveMode)EditorGUILayout.EnumPopup (curveMode, GUILayout.Width (70));
         if (curveMode == MicroSplatProceduralTextureConfig.Layer.CurveMode.Curve)
         {
            if (GUILayout.Button ("E", GUILayout.Width (20)))
            {
               sheetToggle = !sheetToggle;
            }
            curve = EditorGUILayout.CurveField (curve, Color.red, new Rect (0, 0, 1, 1));
         }
         else
         {
            
            EditorGUILayout.BeginVertical ();

            /*
            GUI.enabled = false;
            CopyFilterToAnimCurve (filter, scratchCurve, curveMode);
            EditorGUILayout.CurveField (scratchCurve);
            GUI.enabled = toggle;
            */

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("Center", GUILayout.Width (70));
            filter.center = Mathf.Clamp01 (EditorGUILayout.Slider (filter.center, 0, 1));
            EditorGUILayout.EndHorizontal ();


            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("Width", GUILayout.Width (70));
            filter.width = Mathf.Clamp01 (EditorGUILayout.Slider (filter.width, 0, 1));
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("Contrast", GUILayout.Width (70));
            filter.contrast = EditorGUILayout.Slider (filter.contrast, 0.1f, 10.0f);
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.EndVertical ();
         }

         GUI.enabled = true;
         if (endHorizontal)
         {
            EditorGUILayout.EndHorizontal ();
         }
      }


      class PresetSelection
      {
         public PresetSelection (PresetNames n, MicroSplatProceduralTextureConfig.Layer l)
         {
            layer = l;
            name = n;
         }
         public MicroSplatProceduralTextureConfig.Layer layer;
         public PresetNames name;
      }

      // the GenericMenu.MenuFunction2 event handler for when a menu item is selected
      void OnPresetSelected (object obj)
      {
         PresetSelection ps = (PresetSelection)obj;
         currentPreset = ps.name;
         ps.layer.cavityMapActive = false;
         ps.layer.biomeWeights = new Vector4 (1, 1, 1, 1);
         ps.layer.erosionMapActive = false;
         ps.layer.heightActive = false;
         ps.layer.slopeActive = true;
         ps.layer.noiseActive = false;
         ps.layer.slopeCurve = slopePresets [(int)currentPreset];
         MicroSplatObject.SyncAll ();
      }

      PresetNames currentPreset = PresetNames.Floor;
      int DrawLayer (ref MicroSplatProceduralTextureConfig.ValueMode valueMode, MicroSplatProceduralTextureConfig.Layer layer, int layerIndex, Material mat, bool showBiome, bool showBiome2, bool showCav, bool showErosion)
      {
         int ret = 0;

         using (new EditorGUILayout.VerticalScope ("box"))
         {

            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField (layerIndex.ToString (), GUILayout.Width (20));
            if (GUILayout.Button ("Dupe", GUILayout.Width (60)))
            {
               ret = 3;
            }
            if (GUILayout.Button ("Insert", GUILayout.Width (60)))
            {
               ret = 2;
            }
            if (GUILayout.Button ("Up", GUILayout.Width (60)))
            {
               ret = -1;
            }
            if (GUILayout.Button ("Down", GUILayout.Width (60)))
            {
               ret = 1;
            }
            if (GUILayout.Button ("Delete", GUILayout.Width (60)))
            {
               ret = -2;
            }
            EditorGUILayout.EndHorizontal ();



            Texture2DArray texArray = mat.GetTexture ("_Diffuse") as Texture2DArray;
            layer.textureIndex = MicroSplatUtilities.DrawTextureSelector (layer.textureIndex, texArray);
            EditorGUILayout.Space ();

            layer.weight = EditorGUILayout.Slider ("Weight", layer.weight, 0, 2);

            DrawCurveToggle (ref layer.heightActive, ref layer.heightCurveMode, ref layer.heightCurve, ref layer.heightFilter, "Height", ref layer.heightCurveSheet);
            if (layer.heightActive && layer.heightCurveSheet && layer.heightCurveMode == MicroSplatProceduralTextureConfig.Layer.CurveMode.Curve)
            {
               DrawCurveDataGUI (ref layer.heightCurve, ref valueMode, "Height");
            }

            DrawCurveToggle (ref layer.slopeActive, ref layer.slopeCurveMode, ref layer.slopeCurve, ref layer.slopeFilter, "Slope", ref layer.slopeCurveSheet, false);

            if (layer.slopeCurveMode == MicroSplatProceduralTextureConfig.Layer.CurveMode.Curve)
            {
               if (GUILayout.Button ("P", GUILayout.Width (30)))
               {
                  GenericMenu menu = new GenericMenu ();
                  menu.AddItem (new GUIContent (PresetNames.Floor.ToString ()), currentPreset == PresetNames.Floor, OnPresetSelected, new PresetSelection (PresetNames.Floor, layer));
                  menu.AddItem (new GUIContent (PresetNames.Slopes.ToString ()), currentPreset == PresetNames.Slopes, OnPresetSelected, new PresetSelection (PresetNames.Slopes, layer));
                  menu.AddItem (new GUIContent (PresetNames.Cliff.ToString ()), currentPreset == PresetNames.Cliff, OnPresetSelected, new PresetSelection (PresetNames.Cliff, layer));
                  menu.ShowAsContext ();
               }
            }
            EditorGUILayout.EndHorizontal ();
            if (layer.slopeActive && layer.slopeCurveSheet && layer.slopeCurveMode == MicroSplatProceduralTextureConfig.Layer.CurveMode.Curve)
            {
               DrawCurveDataGUI (ref layer.slopeCurve, ref valueMode, "Slope");
            }


            if (showCav)
            {
               DrawCurveToggle (ref layer.cavityMapActive, ref layer.cavityCurveMode, ref layer.cavityMapCurve, ref layer.cavityMapFilter, "Cavity", ref layer.cavityCurveSheet);
               if (layer.cavityMapActive && layer.cavityCurveSheet && layer.cavityCurveMode == MicroSplatProceduralTextureConfig.Layer.CurveMode.Curve)
               {
                  DrawCurveDataGUI (ref layer.cavityMapCurve, ref valueMode, "Cavity");
               }
            }
            if (showErosion)
            {
               DrawCurveToggle (ref layer.erosionMapActive, ref layer.erosionCurveMode, ref layer.erosionMapCurve, ref layer.erosionFilter, "Erosion", ref layer.erosionCurveSheet);
               if (layer.erosionMapActive && layer.erosionCurveSheet && layer.erosionCurveMode == MicroSplatProceduralTextureConfig.Layer.CurveMode.Curve)
               {
                  DrawCurveDataGUI (ref layer.erosionMapCurve, ref valueMode, "Erosion");
               }
            }


            float old = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 50;
            EditorGUILayout.BeginVertical ("box");
            EditorGUILayout.BeginHorizontal ();

            layer.noiseActive = EditorGUILayout.Toggle (layer.noiseActive, GUILayout.Width (40));
            EditorGUILayout.LabelField ("Noise", GUILayout.Width (75));
            EditorGUILayout.EndHorizontal ();
            GUI.enabled = layer.noiseActive;
            EditorGUILayout.BeginHorizontal ();
            layer.noiseFrequency = EditorGUILayout.FloatField ("Freq", layer.noiseFrequency);
            layer.noiseOffset = EditorGUILayout.FloatField ("Offset", layer.noiseOffset);
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            layer.noiseRange.x = EditorGUILayout.FloatField ("Min", layer.noiseRange.x);
            layer.noiseRange.y = EditorGUILayout.FloatField ("Max", layer.noiseRange.y);
            EditorGUILayout.EndHorizontal ();
            GUI.enabled = true;

            if (showBiome)
            {
               EditorGUILayout.Space ();
               layer.biomeWeights = EditorGUILayout.Vector4Field ("Biome Weights", layer.biomeWeights);
            }
            if (showBiome2)
            {
               layer.biomeWeights2 = EditorGUILayout.Vector4Field ("Biome Weights 2", layer.biomeWeights2);
            }
            EditorGUIUtility.labelWidth = old;
            EditorGUILayout.Space ();
            EditorGUILayout.EndVertical ();
         }
         return ret;

      }

      static GUIContent PCShowProcIndex = new GUIContent ("Procedural Index", "When this texture is painted on the terrain, procedural texturing will be used instead of what's painted");

      public override void DrawShaderGUI (MicroSplatShaderGUI shaderGUI, MicroSplatKeywords keywords, Material mat, MaterialEditor materialEditor, MaterialProperty [] props)
      {
         if (keywords.IsKeywordEnabled ("_DISABLESPLATMAPS"))
            return;
         if ((proceduralTexture || heightGradients || heightHSV) && MicroSplatUtilities.DrawRollup ("Procedural Texture"))
         {
            InitPresets ();
            if (proceduralTexture && combineWithSplat && mat.HasProperty ("_PCShowProceduralIndex"))
            {
               int val = mat.GetInt ("_PCShowProceduralIndex");
               int old = val;
               val = EditorGUILayout.IntField (PCShowProcIndex, val);
               if (old != val)
               {
                  mat.SetInt ("_PCShowProceduralIndex", val);
                  EditorUtility.SetDirty (mat);
               }
            }
            if (proceduralTexture && mat.HasProperty ("_ProcTexNoise"))
            {
               if (mat.HasProperty ("_ProcTexBiomeMask"))
               {
                  materialEditor.TexturePropertySingleLine (CBiomeMask, shaderGUI.FindProp ("_ProcTexBiomeMask", props));
               }
               if (mat.HasProperty ("_ProcTexBiomeMask2"))
               {
                  materialEditor.TexturePropertySingleLine (CBiomeMask2, shaderGUI.FindProp ("_ProcTexBiomeMask2", props));
               }
               if (mat.HasProperty ("_ProcBiomeCurveWeight"))
               {
                  float w = mat.GetFloat ("_ProcBiomeCurveWeight");
                  float nw = EditorGUILayout.Slider (CBiomeCurveWeight, w, 0.01f, 0.5f);
                  if (Mathf.Abs (w - nw) > float.Epsilon)
                  {
                     mat.SetFloat ("_ProcBiomeCurveWeight", nw);
                     EditorUtility.SetDirty (mat);
                  }
               }
               if (noiseType == NoiseType.Texture)
               {
                  materialEditor.TexturePropertySingleLine (CNoiseTex, shaderGUI.FindProp ("_ProcTexNoise", props));
                  MicroSplatUtilities.EnforceDefaultTexture (shaderGUI.FindProp ("_ProcTexNoise", props), "microsplat_def_proctexturenoise");
               }

               Vector4 v = mat.GetVector ("_WorldHeightRange");
               Vector2 range = new Vector2 (v.x, v.y);
               range = EditorGUILayout.Vector2Field (CHeightRange, range);
               if (Mathf.Abs (range.x - v.x) > float.Epsilon || Mathf.Abs (range.y - v.y) > float.Epsilon)
               {
                  mat.SetVector ("_WorldHeightRange", range);
                  EditorUtility.SetDirty (mat);
               }


               var cfg = FindOrCreateProceduralConfig (mat);

               EditorGUI.BeginChangeCheck ();

               DrawArrayHeader (cfg);

               bool showBiomes = keywords.IsKeywordEnabled ("_PCBIOMEMASK") || keywords.IsKeywordEnabled ("_PCBIOMEMASK16");
               bool showBiomes2 = keywords.IsKeywordEnabled ("_PCBIOMEMASK") && keywords.IsKeywordEnabled ("_PCBIOMEMASK2");

               bool showErosion = keywords.IsKeywordEnabled ("_PCFLOW");
               bool showCav = keywords.IsKeywordEnabled ("_PCCAVITY") || keywords.IsKeywordEnabled ("_PCUSECOMBINEDAO");

               for (int i = 0; i < cfg.layers.Count; ++i)
               {
                  int reorder = DrawLayer (ref cfg.valueMode, cfg.layers [i], i, mat, showBiomes, showBiomes2, showCav, showErosion);
                  if (reorder == 3)
                  {
                     var cl = cfg.layers [i];
                     var nl = cl.Copy ();
                     cfg.layers.Insert (i, nl);
                  }
                  if (reorder == -2)
                  {
                     cfg.layers.RemoveAt (i);
                     i--;
                  }
                  else if (reorder == 2)
                  {
                     cfg.layers.Insert (i, new MicroSplatProceduralTextureConfig.Layer ());
                  }
                  else if (reorder < 0 && i > 0)
                  {
                     // move up
                     var l1 = cfg.layers [i];
                     var l2 = cfg.layers [i - 1];
                     cfg.layers [i] = l2;
                     cfg.layers [i - 1] = l1;
                  }
                  else if (reorder > 0 && i < cfg.layers.Count - 1)
                  {
                     // move down
                     var l1 = cfg.layers [i];
                     var l2 = cfg.layers [i + 1];
                     cfg.layers [i] = l2;
                     cfg.layers [i + 1] = l1;
                  }
               }

               if (EditorGUI.EndChangeCheck ())
               {
                  EditorUtility.SetDirty (cfg);
                  MicroSplatObject.SyncAll ();
               }
            }
            if ((heightGradients || heightHSV) && mat.HasProperty ("_WorldHeightRange"))
            {
               Vector4 v = mat.GetVector ("_WorldHeightRange");
               Vector2 range = new Vector2 (v.x, v.y);
               range = EditorGUILayout.Vector2Field (CHeightRange, range);
               if (range.x != v.x || range.y != v.y)
               {
                  mat.SetVector ("_WorldHeightRange", range);
                  EditorUtility.SetDirty (mat);
               }
            }
         }
      }


      public override void InitCompiler (string [] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths [i];
            if (p.EndsWith ("microsplat_properties_proctexture.txt"))
            {
               properties_texturing = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
            if (p.EndsWith ("microsplat_func_proctexture.txt"))
            {
               function_texturing = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
            if (p.EndsWith ("microsplat_func_procgradients.txt"))
            {
               function_gradient = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
            if (p.EndsWith ("microsplat_cbuffer_proctexture.txt"))
            {
               cbuffer_texturing = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
            if (p.EndsWith ("microsplat_cbuffer_procgradients.txt"))
            {
               cbuffer_gradient = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
         }
      }

      public override void WriteProperties (string [] features, System.Text.StringBuilder sb)
      {
         if (proceduralTexture || heightGradients || heightHSV)
         {
            sb.AppendLine ("_WorldHeightRange(\"World Height Range\", Vector) = (0, 500, 0, 0)");
         }
         if (proceduralTexture)
         {
            if (combineWithSplat)
            {
               sb.AppendLine ("      _PCShowProceduralIndex(\"Procedural Texture Index\", Int) = 0");
            }
            sb.AppendLine (properties_texturing.text);

            if (biomeMask != BiomeMaskType.None)
            {
               sb.AppendLine ("      _ProcTexBiomeMask(\"Biome Mask\", 2D) = \"white\" {}");
            }
            if (biomeMask == BiomeMaskType.SplatMap2)
            {
               sb.AppendLine ("      _ProcTexBiomeMask2(\"Biome Mask 2\", 2D) = \"white\" {}");
            }
         }
         if (heightGradients)
         {
            sb.AppendLine ("      _PCHeightGradients(\"Gradients\", 2D) = \"grey\" {}");
         }
         if (heightHSV)
         {
            sb.AppendLine ("      _PCHeightHSV(\"HSV\", 2D) = \"black\" {}");
         }
         if (slopeGradients)
         {
            sb.AppendLine ("      _PCSlopeGradients(\"Gradients\", 2D) = \"grey\" {}");
         }
         if (slopeHSV)
         {
            sb.AppendLine ("      _PCSlopeHSV(\"HSV\", 2D) = \"black\" {}");
         }
         if (cavityMap || erosionMap)
         {
            sb.AppendLine ("      _CavityMap(\"Cavity Map\", 2D) = \"grey\" {}");
         }

      }

      public override void ComputeSampleCounts (string [] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {
         if (proceduralTexture)
         {
            // cant get layer count from here.. grr..
            if (biomeMask != BiomeMaskType.None)
            {
               textureSampleCount++;
            }
            if (biomeMask == BiomeMaskType.SplatMap2)
            {
               textureSampleCount++;
            }
            depTexReadLevel++;
            textureSampleCount += 1;   // noise * layer count

         }
         if (heightGradients)
         {
            textureSampleCount += 4;
         }
         if (heightHSV)
         {
            textureSampleCount += 4;
         }
         if (slopeGradients)
         {
            textureSampleCount += 4;
         }
         if (slopeHSV)
         {
            textureSampleCount += 4;
         }
         if (cavityMap || erosionMap)
         {
            textureSampleCount++;
         }
      }

      public override string [] Pack ()
      {
         List<string> features = new List<string> ();
         if (proceduralTexture)
         {
            features.Add (GetFeatureName (DefineFeature._PROCEDURALTEXTURE));

            if (combineWithSplat)
            {
               features.Add (GetFeatureName (DefineFeature._PROCEDURALBLENDSPLATS));
            }

            if (noiseType == NoiseType.Procedural)
            {
               features.Add (GetFeatureName (DefineFeature._PCNOISEPROCEDURAL));
            }

            if (biomeMask == BiomeMaskType.SplatMap)
            {
               features.Add (GetFeatureName (DefineFeature._PCBIOMEMASK));
            }
            else if (biomeMask == BiomeMaskType.ExclusionMap)
            {
               features.Add (GetFeatureName (DefineFeature._PCBIOMEMASK16));
            }
            else if (biomeMask == BiomeMaskType.SplatMap2)
            {
               features.Add (GetFeatureName (DefineFeature._PCBIOMEMASK));
               features.Add (GetFeatureName (DefineFeature._PCBIOMEMASK2));
            }

            if (noiseSpace == NoiseSpace.UV)
            {
               features.Add (GetFeatureName (DefineFeature._PCNOISEUV));
            }
            else if (noiseSpace == NoiseSpace.Triplanar)
            {
               features.Add (GetFeatureName (DefineFeature._PCNOISETRIPLANAR));
            }

            if (cavityMap)
            {
               features.Add (GetFeatureName (DefineFeature._PCCAVITY));
            }
            if (erosionMap)
            {
               features.Add (GetFeatureName (DefineFeature._PCFLOW));
            }

            if (normalSource == CombinedNormalSource.CombinedNormal)
            {
               features.Add (GetFeatureName (DefineFeature._PCUSECOMBINEDNORMAL));
            }
            if (heightSource == CombinedHeightSource.CombinedHeightMap)
            {
               features.Add (GetFeatureName (DefineFeature._PCUSECOMBINEDHEIGHT));
            }
            if (cavitySource == CombinedCavitySource.CombinedAOChannel)
            {
               features.Add (GetFeatureName (DefineFeature._PCUSECOMBINEDAO));
            }
         }

         if (heightGradients)
         {
            features.Add (GetFeatureName (DefineFeature._PCHEIGHTGRADIENT));
         }
         if (heightHSV)
         {
            features.Add (GetFeatureName (DefineFeature._PCHEIGHTHSV));
         }
         if (slopeGradients)
         {
            features.Add (GetFeatureName (DefineFeature._PCSLOPEGRADIENT));
         }
         if (slopeHSV)
         {
            features.Add (GetFeatureName (DefineFeature._PCSLOPEHSV));
         }
         return features.ToArray ();
      }

      public override void WritePerMaterialCBuffer (string [] features, System.Text.StringBuilder sb)
      {
         if (proceduralTexture || heightGradients || heightHSV)
         {
            sb.AppendLine ("      float2 _WorldHeightRange;");
         }
         if (proceduralTexture)
         {
            sb.AppendLine (cbuffer_texturing.text);
         }
         if (heightGradients || heightHSV || slopeGradients || slopeHSV)
         {
            sb.AppendLine (cbuffer_gradient.text);
         }
      }

      public override void WriteFunctions (string [] features, System.Text.StringBuilder sb)
      {
         if (proceduralTexture)
         {
            sb.AppendLine (function_texturing.text);
         }
         if (heightGradients || heightHSV || slopeGradients || slopeHSV)
         {
            sb.AppendLine (function_gradient.text);
         }
      }

      public override void Unpack (string [] keywords)
      {
         proceduralTexture = HasFeature (keywords, DefineFeature._PROCEDURALTEXTURE);
         if (proceduralTexture)
         {
            combineWithSplat = HasFeature (keywords, DefineFeature._PROCEDURALBLENDSPLATS);
            biomeMask = HasFeature (keywords, DefineFeature._PCBIOMEMASK) ? BiomeMaskType.SplatMap : BiomeMaskType.None;
            if (HasFeature (keywords, DefineFeature._PCBIOMEMASK16))
            {
               biomeMask = BiomeMaskType.ExclusionMap;
            }
            if (HasFeature (keywords, DefineFeature._PCBIOMEMASK2))
            {
               biomeMask = BiomeMaskType.SplatMap2;
            }

            noiseSpace = NoiseSpace.World;
            if (HasFeature (keywords, DefineFeature._PCNOISEUV))
            {
               noiseSpace = NoiseSpace.UV;
            }
            else if (HasFeature (keywords, DefineFeature._PCNOISETRIPLANAR))
            {
               noiseSpace = NoiseSpace.Triplanar;
            }
            noiseType = HasFeature (keywords, DefineFeature._PCNOISEPROCEDURAL) ? NoiseType.Procedural : NoiseType.Texture;

            cavityMap = HasFeature (keywords, DefineFeature._PCCAVITY);
            erosionMap = HasFeature (keywords, DefineFeature._PCFLOW);
            normalSource = HasFeature (keywords, DefineFeature._PCUSECOMBINEDNORMAL) ? CombinedNormalSource.CombinedNormal : CombinedNormalSource.VertexNormal;
            heightSource = HasFeature (keywords, DefineFeature._PCUSECOMBINEDHEIGHT) ? CombinedHeightSource.CombinedHeightMap : CombinedHeightSource.WorldHeight;
            cavitySource = HasFeature (keywords, DefineFeature._PCUSECOMBINEDAO) ? CombinedCavitySource.CombinedAOChannel : CombinedCavitySource.CavityMap;
         }
         heightGradients = HasFeature (keywords, DefineFeature._PCHEIGHTGRADIENT);
         heightHSV = HasFeature (keywords, DefineFeature._PCHEIGHTHSV);
         slopeGradients = HasFeature (keywords, DefineFeature._PCSLOPEGRADIENT);
         slopeHSV = HasFeature (keywords, DefineFeature._PCSLOPEHSV);
      }

      Gradient NewGradient ()
      {
         Gradient g = new Gradient ();
         g.mode = GradientMode.Blend;
         var colors = new GradientColorKey [1];
         var alphas = new GradientAlphaKey [1];
         alphas [0].alpha = 1;
         colors [0].color = Color.grey;

         if (PlayerSettings.colorSpace == ColorSpace.Linear)
         {
            colors [0].color = colors [0].color.gamma;
         }

         g.colorKeys = colors;
         g.alphaKeys = alphas;
         return g;
      }



      static GUIContent CHeightGradientTint = new GUIContent ("Height Gradient Tint", "Gradient tint over height of the terrain");
      static GUIContent CHeightGradientHue = new GUIContent ("Height Gradient Hue", "Gradient hue over height of the terrain");
      static GUIContent CHeightGradientHSL = new GUIContent ("Height Gradient HSL", "HSL adjustment over height of the terrain");

      static GUIContent CHeightGradientSaturation = new GUIContent ("Height Gradient Saturation", "Gradient saturation over height of the terrain");
      static GUIContent CHeightGradientBrightness = new GUIContent ("Height Gradient Brightness", "Gradient brightness over height of the terrain");

      static GUIContent CSlopeGradientTint = new GUIContent ("Slope Gradient Tint", "Gradient tint over slope (-1 to 1) of the terrain");
      static GUIContent CSlopeGradientHue = new GUIContent ("Slope Gradient Hue", "Gradient hue over slope (-1 to 1) of the terrain");
      static GUIContent CSlopeGradientHSL = new GUIContent ("Slope Gradient HSL", "HSL adjustment based on the slope of the terrain");
      static GUIContent CSlopeGradientSaturation = new GUIContent ("Slope Gradient Saturation", "Gradient saturation over slope (-1 to 1) of the terrain");
      static GUIContent CSlopeGradientBrightness = new GUIContent ("Slope Gradient Brightness", "Gradient brightness over slope (-1 to 1) of the terrain");

      public override void DrawPerTextureGUI (int index, MicroSplatKeywords keywords, Material mat, MicroSplatPropData propData)
      {
         EditorGUI.BeginChangeCheck ();
         // this isn't really a proper per texture GUI, but it works well here, so lets do it that way..
         var config = FindOrCreateProceduralConfig (mat);

         while (config.heightGradients.Count <= index)
         {
            config.heightGradients.Add (NewGradient ());
            EditorUtility.SetDirty (config);
         }

         while (config.heightHSV.Count <= index)
         {
            config.heightHSV.Add (new MicroSplatProceduralTextureConfig.HSVCurve ());
            EditorUtility.SetDirty (config);
         }
         while (config.slopeGradients.Count <= index)
         {
            config.slopeGradients.Add (NewGradient ());
            EditorUtility.SetDirty (config);
         }

         while (config.slopeHSV.Count <= index)
         {
            config.slopeHSV.Add (new MicroSplatProceduralTextureConfig.HSVCurve ());
            EditorUtility.SetDirty (config);
         }

         SerializedObject so = new SerializedObject (config);
         so.Update ();


         // height based
         {
            // gradient tint
            EditorGUILayout.BeginHorizontal ();

            bool enabled = keywords.IsKeywordEnabled (GetFeatureName (DefineFeature._PCHEIGHTGRADIENT));
            bool newEnabled = EditorGUILayout.Toggle (enabled, GUILayout.Width (20));
            if (enabled != newEnabled)
            {
               if (newEnabled)
                  keywords.EnableKeyword (GetFeatureName (DefineFeature._PCHEIGHTGRADIENT));
               else
                  keywords.DisableKeyword (GetFeatureName (DefineFeature._PCHEIGHTGRADIENT));
               heightGradients = newEnabled;
            }

            GUI.enabled = enabled;


            var list = so.FindProperty ("heightGradients");
            EditorGUI.BeginChangeCheck ();
            EditorGUILayout.PropertyField (list.GetArrayElementAtIndex (index), CHeightGradientTint);

            if (EditorGUI.EndChangeCheck ())
            {
               so.ApplyModifiedProperties ();
               so.Update ();
            }

            if (GUILayout.Button ("Reset", GUILayout.Width (60)))
            {
               config.heightGradients [index] = NewGradient ();
               EditorUtility.SetDirty (config);
               so.Update ();
            }


            GUI.enabled = true;
            EditorGUILayout.EndHorizontal ();


            // HSV
            EditorGUILayout.BeginHorizontal ();

            enabled = keywords.IsKeywordEnabled (GetFeatureName (DefineFeature._PCHEIGHTHSV));
            newEnabled = EditorGUILayout.Toggle (enabled, GUILayout.Width (20));
            if (enabled != newEnabled)
            {
               if (newEnabled)
                  keywords.EnableKeyword (GetFeatureName (DefineFeature._PCHEIGHTHSV));
               else
                  keywords.DisableKeyword (GetFeatureName (DefineFeature._PCHEIGHTHSV));
               heightHSV = newEnabled;
            }

            GUI.enabled = enabled;

            var hsvRect = new Rect (0, -1, 1, 2);
            config.heightHSV [index].H = EditorGUILayout.CurveField (enabled ? CHeightGradientHue : CHeightGradientHSL, config.heightHSV [index].H, Color.yellow, hsvRect);
            if (GUILayout.Button ("Reset", GUILayout.Width (60)))
            {
               config.heightHSV [index].H = AnimationCurve.Linear (0, 0.0f, 1, 0.0f);
            }
            EditorGUILayout.EndHorizontal ();
            if (enabled)
            {
               EditorGUILayout.BeginHorizontal ();
               EditorGUILayout.LabelField ("", GUILayout.Width (noPerTexToggleWidth));
               config.heightHSV [index].S = EditorGUILayout.CurveField (CHeightGradientSaturation, config.heightHSV [index].S, Color.blue, hsvRect);
               if (GUILayout.Button ("Reset", GUILayout.Width (60)))
               {
                  config.heightHSV [index].S = AnimationCurve.Linear (0, 0.0f, 1, 0.0f);
               }
               EditorGUILayout.EndHorizontal ();

               EditorGUILayout.BeginHorizontal ();
               EditorGUILayout.LabelField ("", GUILayout.Width (noPerTexToggleWidth));
               config.heightHSV [index].V = EditorGUILayout.CurveField (CHeightGradientBrightness, config.heightHSV [index].V, Color.grey, hsvRect);
               if (GUILayout.Button ("Reset", GUILayout.Width (60)))
               {
                  config.heightHSV [index].V = AnimationCurve.Linear (0, 0.0f, 1, 0.0f);
               }
               EditorGUILayout.EndHorizontal ();
            }
            GUI.enabled = true;
         }
         // slope based
         {
            // gradient tint
            EditorGUILayout.BeginHorizontal ();

            bool enabled = keywords.IsKeywordEnabled (GetFeatureName (DefineFeature._PCSLOPEGRADIENT));
            bool newEnabled = EditorGUILayout.Toggle (enabled, GUILayout.Width (20));
            if (enabled != newEnabled)
            {
               if (newEnabled)
                  keywords.EnableKeyword (GetFeatureName (DefineFeature._PCSLOPEGRADIENT));
               else
                  keywords.DisableKeyword (GetFeatureName (DefineFeature._PCSLOPEGRADIENT));
               slopeGradients = newEnabled;
            }

            GUI.enabled = enabled;


            var list = so.FindProperty ("slopeGradients");
            EditorGUI.BeginChangeCheck ();
            EditorGUILayout.PropertyField (list.GetArrayElementAtIndex (index), CSlopeGradientTint);

            if (EditorGUI.EndChangeCheck ())
            {
               so.ApplyModifiedProperties ();
               so.Update ();
            }

            if (GUILayout.Button ("Reset", GUILayout.Width (60)))
            {
               config.slopeGradients [index] = NewGradient ();
               EditorUtility.SetDirty (config);
               so.Update ();
            }


            GUI.enabled = true;
            EditorGUILayout.EndHorizontal ();


            // HSV
            EditorGUILayout.BeginHorizontal ();

            enabled = keywords.IsKeywordEnabled (GetFeatureName (DefineFeature._PCSLOPEHSV));
            newEnabled = EditorGUILayout.Toggle (enabled, GUILayout.Width (20));
            if (enabled != newEnabled)
            {
               if (newEnabled)
                  keywords.EnableKeyword (GetFeatureName (DefineFeature._PCSLOPEHSV));
               else
                  keywords.DisableKeyword (GetFeatureName (DefineFeature._PCSLOPEHSV));
               slopeHSV = newEnabled;
            }

            GUI.enabled = enabled;

            var hsvRect = new Rect (0, -1, 1, 2);
            config.slopeHSV [index].H = EditorGUILayout.CurveField (enabled ? CSlopeGradientHue : CSlopeGradientHSL, config.slopeHSV [index].H, Color.yellow, hsvRect);
            if (GUILayout.Button ("Reset", GUILayout.Width (60)))
            {
               config.slopeHSV [index].H = AnimationCurve.Linear (0, 0.0f, 1, 0.0f);
               MicroSplatObject.SyncAll ();
            }
            EditorGUILayout.EndHorizontal ();
            if (enabled)
            {
               EditorGUILayout.BeginHorizontal ();
               EditorGUILayout.LabelField ("", GUILayout.Width (noPerTexToggleWidth));
               config.slopeHSV [index].S = EditorGUILayout.CurveField (CSlopeGradientSaturation, config.slopeHSV [index].S, Color.blue, hsvRect);
               if (GUILayout.Button ("Reset", GUILayout.Width (60)))
               {
                  config.slopeHSV [index].S = AnimationCurve.Linear (0, 0.0f, 1, 0.0f);
                  MicroSplatObject.SyncAll ();
               }
               EditorGUILayout.EndHorizontal ();

               EditorGUILayout.BeginHorizontal ();
               EditorGUILayout.LabelField ("", GUILayout.Width (noPerTexToggleWidth));
               config.slopeHSV [index].V = EditorGUILayout.CurveField (CSlopeGradientBrightness, config.slopeHSV [index].V, Color.grey, hsvRect);
               if (GUILayout.Button ("Reset", GUILayout.Width (60)))
               {
                  config.slopeHSV [index].V = AnimationCurve.Linear (0, 0.0f, 1, 0.0f);
                  MicroSplatObject.SyncAll ();
               }
               EditorGUILayout.EndHorizontal ();
            }
            GUI.enabled = true;
         }
         if (EditorGUI.EndChangeCheck ())
         {
            MicroSplatObject.SyncAll ();
            EditorUtility.SetDirty (config);

         }
         if (so.ApplyModifiedProperties ())
         {
            so.Update ();
            MicroSplatObject.SyncAll ();
         }
      }
   }
#endif


}
