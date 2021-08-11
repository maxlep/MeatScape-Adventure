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
   public class MicroSplatMeshModule : FeatureDescriptor
   {
      const string sDefine = "__MICROSPLAT_MESH__";
      static MicroSplatMeshModule()
      {
         MicroSplatDefines.InitDefine(sDefine);
      }
      [PostProcessSceneAttribute (0)]
      public static void OnPostprocessScene()
      { 
         MicroSplatDefines.InitDefine(sDefine);
      }

      public override int DisplaySortOrder()
      {
         return -999;
      }

      public override string ModuleName()
      {
         return "Mesh";
      }

      public override bool HideModule ()
      {
         return meshWorkflow == DataSource.None;
      }

      public override string GetHelpPath ()
      {
         return "/MicroSplat/Core/Documentation/MicroSplat - Mesh Workflow.pdf";
      }

      public enum DefineFeature
      {
         _MESHOVERLAYSPLATS,
         _MESHCOMBINED,
         _MESHCOMBINEDEMISSION,
         _MESHCOMBINEDSMOOTHMETAL,
         _MESHCOMBINEDHEIGHT,
         _MESHCOMBINEDOCCLUSION,
         _MESHCOMBINEDPACKEDMAP,
         _MESHCOMBINEDSPECULARMAP,
         _MESHCOMBINENORMALREPLACE,
         _MESHCOMBINEDDETAILALBEDO,
         _MESHCOMBINEDDETAILNORMAL,
         _MESHCOMBINEDSSSMAP,
         _MESHCOMBINEDUSESSS,
         _MESHSUBARRAY,
         _MESHUV2,
         _FORCELOCALSPACE,
         _DISABLESPLATMAPS,
         _MESHCOMBINEDPERTEXNORMALBLEND,
         _DISPLACEMENTDAMPENING,
         kNumFeatures,
      }
         

      public enum PBRMapPacking
      {
         Packed,
         Separate
      }

      public enum ShaderType
      {
         SplatMap,
         Overlay,
         Combined
      }

      public enum UVSource
      {
         TextureUV,
         LightmapUV,
      }

      public enum SSSMode
      {
         None,
         Value,
         Texture
      }

      public enum DataSource
      {
         None,
         Texture,
         Vertex
      }
         
      public ShaderType shaderType = ShaderType.SplatMap;
      public bool combinedEmission;
      public bool combinedSmoothMetal;
      public bool combinedHeight;
      public bool combinedOcclusion;
      public bool combinedDetailAlbedo;
      public bool combinedDetailNormal;
      public bool combinedSpecular;
      public SSSMode sssMode = SSSMode.None;
      public bool subArray;
      public bool forceLocalSpace;
      public bool disableSplatMaps;
      public UVSource uvSource = UVSource.TextureUV;
      public DataSource meshWorkflow = DataSource.None;
      public PBRMapPacking PBRPacking = PBRMapPacking.Packed;
      public bool perTexNormalBlend;
   #if __MICROSPLAT_TESSELLATION__
      public bool displacementDampening;
   #endif
      GUIContent CShaderType = new GUIContent("Mesh Shader Mode", "SplatMap : Traditional Splat map shader\nOverlay : Allows you to blend textures onto existing materials\nCombined : A combined shader, with options for regular textures");
      GUIContent CSubArray = new GUIContent("Use Sub Array", "Use 4 textures from a texture array to reduce the number of control maps needed.");
      GUIContent CUVSource = new GUIContent("Source UVs", "Source UVs for mesh, should be unique, non-overlapping UVs in 0 to 1 space");
      GUIContent CForceLocalSpace = new GUIContent("Force Local Space", "Forces world position and world normal into local space for all calculations");
      GUIContent CDisableSplatMaps = new GUIContent ("Disable Splat Maps", "Disable Splat maps, for when we want the combined or overlay shader but only want effects to show on it, no splat mapping");


   #if __MICROSPLAT_TESSELLATION__
      GUIContent CDisplacementDampening = new GUIContent ("Displacement Dampening", "Allows you to paint where displacement should be removed to avoid cracking");
   #endif

      // Can we template these somehow?
      public static string GetFeatureName(DefineFeature feature)
      {
         return System.Enum.GetName(typeof(DefineFeature), feature);
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

      public static bool HasFeature(string[] keywords, string f)
      {
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

      static GUIContent CPBRPacking = new GUIContent ("Packing Mode", "Should we expect separate maps, or a map with PBR inputs pack together");
      static GUIContent CCombinedSmoothMetal = new GUIContent("Enabled Smooth/Metal Map", "Use texture data for smoothness, and metal maps");
      static GUIContent CCombinedEmission = new GUIContent("Enable Emission Map", "Enable a texture based emission map");
      static GUIContent CCombinedHeight = new GUIContent("Enable Height Map", "Enable a height map texture");
      static GUIContent CCombinedOcclusion = new GUIContent ("Enable Occlusion Map", "Enable an occlusion map texture");
      static GUIContent CCombinedDetailAlbedo = new GUIContent ("Enable Detail Albedo", "Detail Map for Albedo");
      static GUIContent CCombinedDetailNormal = new GUIContent ("Enable Detail Normal", "Detail Map for Normals");
      static GUIContent CCombinedSSS = new GUIContent ("Subsurface Scattering Mode", "Do you want subsurface scattering? And do you want a consistent thickness or to provide a thickness map?");
      static GUIContent CCombinedSpecular = new GUIContent ("Enabled Specular Map", "Use a specular map");

      public override void DrawFeatureGUI(MicroSplatKeywords keywords)
      {
         if (meshWorkflow == DataSource.None)
            return;

         {
            if (meshWorkflow == DataSource.Texture)
            {
               uvSource = (UVSource)EditorGUILayout.EnumPopup (CUVSource, uvSource);
            }
            shaderType = (ShaderType)EditorGUILayout.EnumPopup(CShaderType, shaderType);
            forceLocalSpace = EditorGUILayout.Toggle(CForceLocalSpace, forceLocalSpace);

            if (shaderType == ShaderType.Combined)
            {
               EditorGUI.indentLevel++;
               PBRPacking = (PBRMapPacking)EditorGUILayout.EnumPopup(CPBRPacking, PBRPacking);
               if (PBRPacking == PBRMapPacking.Separate)
               {
                  combinedSmoothMetal = EditorGUILayout.Toggle (CCombinedSmoothMetal, combinedSmoothMetal);
                  combinedHeight = EditorGUILayout.Toggle (CCombinedHeight, combinedHeight);
                  combinedOcclusion = EditorGUILayout.Toggle (CCombinedOcclusion, combinedOcclusion);
               }
               if (keywords.IsKeywordEnabled("_USESPECULARWORKFLOW"))
               {
                  combinedSpecular = EditorGUILayout.Toggle (CCombinedSpecular, combinedSpecular);
               }
               combinedEmission = EditorGUILayout.Toggle(CCombinedEmission, combinedEmission);
               combinedDetailAlbedo = EditorGUILayout.Toggle (CCombinedDetailAlbedo, combinedDetailAlbedo);
               combinedDetailNormal = EditorGUILayout.Toggle (CCombinedDetailNormal, combinedDetailNormal);
               sssMode = (SSSMode)EditorGUILayout.EnumPopup (CCombinedSSS, sssMode);
               disableSplatMaps = EditorGUILayout.Toggle (CDisableSplatMaps, disableSplatMaps);
               
               EditorGUI.indentLevel--;
            }
            if (meshWorkflow == DataSource.Texture)
            {
               subArray = EditorGUILayout.Toggle (CSubArray, subArray);
#if __MICROSPLAT_TESSELLATION__
               displacementDampening = EditorGUILayout.Toggle (CDisplacementDampening, displacementDampening);
#endif
            }

         }
      }
         
      static TextAsset combinedFunc;
      static TextAsset vertexFunc;
      static TextAsset combinedCBuffer;

      public override string[] Pack()
      {
         List<string> features = new List<string>();
         if (meshWorkflow != DataSource.None)
         {
            if (meshWorkflow == DataSource.Texture)
            {
               if (uvSource == UVSource.LightmapUV)
               {
                  features.Add (GetFeatureName (DefineFeature._MESHUV2));
               }
               if (subArray)
               {
                  features.Add (GetFeatureName (DefineFeature._MESHSUBARRAY));
               }
#if __MICROSPLAT_TESSELLATION__
               if (displacementDampening)
               {
                  features.Add (GetFeatureName (DefineFeature._DISPLACEMENTDAMPENING));
               }
#endif
            }
            if (forceLocalSpace)
            {
               features.Add (GetFeatureName (DefineFeature._FORCELOCALSPACE));
            }
            if (shaderType == ShaderType.Overlay)
            {
               features.Add(GetFeatureName(DefineFeature._MESHOVERLAYSPLATS));
            }
            if (shaderType == ShaderType.Combined)
            {
               features.Add(GetFeatureName(DefineFeature._MESHCOMBINED));

               if (PBRPacking == PBRMapPacking.Separate)
               {
                  if (combinedSmoothMetal)
                  {
                     features.Add (GetFeatureName (DefineFeature._MESHCOMBINEDSMOOTHMETAL));
                  }
                  if (combinedHeight)
                  {
                     features.Add (GetFeatureName (DefineFeature._MESHCOMBINEDHEIGHT));
                  }
                  if (combinedOcclusion)
                  {
                     features.Add (GetFeatureName (DefineFeature._MESHCOMBINEDOCCLUSION));
                  }
                  if (perTexNormalBlend)
                  {
                     features.Add (GetFeatureName (DefineFeature._MESHCOMBINEDPERTEXNORMALBLEND));
                  }
               }
               else
               {
                  features.Add (GetFeatureName (DefineFeature._MESHCOMBINEDPACKEDMAP));
               }

               if (combinedSpecular)
               {
                  features.Add (GetFeatureName (DefineFeature._MESHCOMBINEDSPECULARMAP));
               }
               if (combinedEmission)
               {
                  features.Add(GetFeatureName(DefineFeature._MESHCOMBINEDEMISSION));
               }
               if (combinedDetailAlbedo)
               {
                  features.Add (GetFeatureName (DefineFeature._MESHCOMBINEDDETAILALBEDO));
               }
               if (combinedDetailNormal)
               {
                  features.Add (GetFeatureName (DefineFeature._MESHCOMBINEDDETAILNORMAL));
               }
               if (sssMode == SSSMode.Value || sssMode == SSSMode.Texture)
               {
                  features.Add (GetFeatureName (DefineFeature._MESHCOMBINEDUSESSS));
                  if (sssMode == SSSMode.Texture)
                  {
                     features.Add (GetFeatureName (DefineFeature._MESHCOMBINEDSSSMAP));
                  }
               }

               if (disableSplatMaps)
               {
                  features.Add(GetFeatureName (DefineFeature._DISABLESPLATMAPS));
               }   
            }


         }
         return features.ToArray();
      }

      public override void Unpack(string[] keywords)
      {
         meshWorkflow = DataSource.None;
         if (HasFeature (keywords, MicroSplatBaseFeatures.GetFeatureName (MicroSplatBaseFeatures.DefineFeature._MICROMESH)))
         {
            meshWorkflow = DataSource.Texture;
         }
         if (HasFeature (keywords, MicroSplatBaseFeatures.GetFeatureName (MicroSplatBaseFeatures.DefineFeature._MICROVERTEXMESH)))
         {
            meshWorkflow = DataSource.Vertex;
         }

         shaderType = (HasFeature(keywords, DefineFeature._MESHOVERLAYSPLATS)) ? ShaderType.Overlay : ShaderType.SplatMap;
         if (HasFeature(keywords, DefineFeature._MESHCOMBINED))
         {
            shaderType = ShaderType.Combined;
            perTexNormalBlend = HasFeature (keywords, DefineFeature._MESHCOMBINEDPERTEXNORMALBLEND);
         }
         if (HasFeature (keywords, DefineFeature._MESHCOMBINEDPACKEDMAP))
         {
            PBRPacking = PBRMapPacking.Packed;
         }
         else
         {
            PBRPacking = PBRMapPacking.Separate;
            combinedSmoothMetal = HasFeature (keywords, DefineFeature._MESHCOMBINEDSMOOTHMETAL);
            combinedHeight = HasFeature (keywords, DefineFeature._MESHCOMBINEDHEIGHT);
            combinedOcclusion = HasFeature (keywords, DefineFeature._MESHCOMBINEDOCCLUSION);
            
         }
         combinedSpecular = HasFeature (keywords, DefineFeature._MESHCOMBINEDSPECULARMAP);
         combinedEmission = HasFeature (keywords, DefineFeature._MESHCOMBINEDEMISSION);
         combinedDetailAlbedo = HasFeature (keywords, DefineFeature._MESHCOMBINEDDETAILALBEDO);
         combinedDetailNormal = HasFeature (keywords, DefineFeature._MESHCOMBINEDDETAILNORMAL);
         sssMode = SSSMode.None;
         if (HasFeature(keywords, DefineFeature._MESHCOMBINEDUSESSS))
         {
            sssMode = SSSMode.Value;
            if (HasFeature(keywords, DefineFeature._MESHCOMBINEDSSSMAP))
            {
               sssMode = SSSMode.Texture;
            }
         }
         subArray = HasFeature(keywords, DefineFeature._MESHSUBARRAY);
         uvSource = HasFeature(keywords, DefineFeature._MESHUV2) ? UVSource.LightmapUV : UVSource.TextureUV;
         forceLocalSpace = HasFeature(keywords, DefineFeature._FORCELOCALSPACE);
         disableSplatMaps = HasFeature(keywords, DefineFeature._DISABLESPLATMAPS) && shaderType == ShaderType.Combined;
   #if __MICROSPLAT_TESSELLATION__
         displacementDampening = HasFeature(keywords, DefineFeature._DISPLACEMENTDAMPENING);
   #endif
      }

      public override void InitCompiler(string[] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths[i];
            if (p.EndsWith("microsplat_meshcombined_func.txt"))
            {
               combinedFunc = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith ("microsplat_meshcombined_cbuffer.txt"))
            {
               combinedCBuffer = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
            if (p.EndsWith ("microsplat_vertexmodule_func.txt"))
            {
               vertexFunc = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
         }
      }

      static GUIContent CStandardDiffuse = new GUIContent("Albedo", "Albedo texture");
      static GUIContent CStandardNormal = new GUIContent("Normal", "Normal Map");
      static GUIContent CStandardSmoothMetal = new GUIContent("Smoothness/Metal", "Smoothness Metal texture");
      static GUIContent CStandardEmission = new GUIContent("Emission", "Emissive texture");
      static GUIContent CStandardOcclusion = new GUIContent("Occlusion", "Occlusion texture");
      static GUIContent CStandardHeight = new GUIContent("Height", "Height texture");
      static GUIContent CStandardSpecular = new GUIContent ("Specular", "Specular Texture");
      static GUIContent CStandardPackedMap = new GUIContent ("Packed Map", "(R) Metallic, (G) Smoothness, (B) Height (A) Occlusion");
      static GUIContent CStandardSSS = new GUIContent ("Subsurface Scattering", "Subsurface Scattering map (G) thickness");
      static GUIContent CStandardDetailAlbedo = new GUIContent ("Detail Albedo x2", "Albedo texture");
      static GUIContent CStandardDetailNormal = new GUIContent ("Detail Normal", "Normal texture");
      static GUIContent CStandardDetailUVScale = new GUIContent ("Detail UV Scale/Offset", "Scale and offset for UVs on the detail textures");
      static GUIContent CStandardDetailNormalScale = new GUIContent ("Detail Normal Strength", "Strength of detail normal");
#if __MICROSPLAT_TESSELLATION__
      static GUIContent CTessellationBlend = new GUIContent("Tessellation Blend", "Blend between base map and splat map tessellation. 0 is just the base map, 0.5 is mixed, 1 is just splats");
      static GUIContent CTessellationOffset = new GUIContent("Tessellation Offset", "Center point for tessellation displacement, use the center button to compute the ideal offset");
#endif


      public override void DrawShaderGUI(MicroSplatShaderGUI shaderGUI, MicroSplatKeywords keywords, Material mat, MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if (keywords.IsKeywordEnabled ("_MICROMESH"))
         {
            if (keywords.IsKeywordEnabled ("_MESHOVERLAYSPLATS"))
            {
               if (MicroSplatUtilities.DrawRollup ("Mesh Overlay"))
               {
                  if (mat.HasProperty ("_MeshAlphaIndex"))
                  {
                     materialEditor.ShaderProperty (shaderGUI.FindProp ("_MeshAlphaIndex", props), "Mesh Alpha Index");
                  }
               }
            }
         }
         if (keywords.IsKeywordEnabled ("_MESHCOMBINED"))
         {
            if (MicroSplatUtilities.DrawRollup ("Mesh Combined"))
            {
               if (disableSplatMaps)
               {
                  // make sure alpha index is 0 in no splat case
                  if (mat.HasProperty ("_MeshAlphaIndex"))
                  {
                     if (mat.GetInt ("_MeshAlphaIndex") != 0)
                     {
                        mat.SetInt ("_MeshAlphaIndex", 0);
                        EditorUtility.SetDirty (mat);
                     }
                  }
               }
               else
               {
                  if (mat.HasProperty ("_MeshAlphaIndex"))
                  {
                     materialEditor.ShaderProperty (shaderGUI.FindProp ("_MeshAlphaIndex", props), "Mesh Alpha Index");
                  }
               }
               if (mat.HasProperty ("_StandardDiffuse"))
               {
                  EditorGUILayout.BeginHorizontal ();
                  materialEditor.TexturePropertySingleLine (CStandardDiffuse, shaderGUI.FindProp ("_StandardDiffuse", props));
                  var colorProp = shaderGUI.FindProp ("_StandardDiffuseTint", props);
                  var c = colorProp.colorValue;
                  var nc = EditorGUILayout.ColorField (c, GUILayout.Width (60));
                  if (c != nc)
                  {
                     colorProp.colorValue = nc;
                  }
                  EditorGUILayout.EndHorizontal ();
                  materialEditor.TexturePropertySingleLine (CStandardNormal, shaderGUI.FindProp ("_StandardNormal", props));
               }
               if (mat.HasProperty ("_StandardSmoothMetal"))
               {
                  materialEditor.TexturePropertySingleLine (CStandardSmoothMetal, shaderGUI.FindProp ("_StandardSmoothMetal", props));
               }
               if (mat.HasProperty ("_StandardSmoothness"))
               {
                  materialEditor.ShaderProperty (shaderGUI.FindProp ("_StandardSmoothness", props), "Smoothness");
                  materialEditor.ShaderProperty (shaderGUI.FindProp ("_StandardMetal", props), "Metallic");
               }
               if (mat.HasProperty ("_StandardHeight"))
               {
                  materialEditor.TexturePropertySingleLine (CStandardHeight, shaderGUI.FindProp ("_StandardHeight", props));
               }
               if (mat.HasProperty ("_StandardSpecular"))
               {
                  materialEditor.TexturePropertySingleLine (CStandardSpecular, shaderGUI.FindProp ("_StandardSpecular", props));
               }
               if (mat.HasProperty ("_StandardEmission"))
               {
                  materialEditor.TexturePropertySingleLine (CStandardEmission, shaderGUI.FindProp ("_StandardEmission", props));
               }
               if (mat.HasProperty ("_StandardOcclusion"))
               {
                  materialEditor.TexturePropertySingleLine (CStandardOcclusion, shaderGUI.FindProp ("_StandardOcclusion", props));
               }
               if (mat.HasProperty ("_StandardPackedMap"))
               {
                  materialEditor.TexturePropertySingleLine (CStandardPackedMap, shaderGUI.FindProp ("_StandardPackedMap", props));
               }
               if (mat.HasProperty ("_StandardSSSMap"))
               {
                  materialEditor.TexturePropertySingleLine (CStandardSSS, shaderGUI.FindProp ("_StandardSSSMap", props));
               }
               if (mat.HasProperty ("_StandardSSSValue"))
               {
                  materialEditor.RangeProperty (shaderGUI.FindProp ("_StandardSSSValue", props), "Subsurface Thickness");
               }
               if (mat.HasProperty ("_StandardSSSTint"))
               {
                  materialEditor.ColorProperty (shaderGUI.FindProp ("_StandardSSSTint", props), "Subsurface Tint");
               }
               if (mat.HasProperty ("_StandardSSSBlend"))
               {
                  materialEditor.RangeProperty (shaderGUI.FindProp ("_StandardSSSBlend", props), "Subsurface Blend");
               }
               if (mat.HasProperty ("_StandardDetailAlbedo"))
               {
                  materialEditor.TexturePropertySingleLine (CStandardDetailAlbedo, shaderGUI.FindProp ("_StandardDetailAlbedo", props));
               }
               if (mat.HasProperty ("_StandardDetailNormal"))
               {
                  materialEditor.TexturePropertySingleLine (CStandardDetailNormal, shaderGUI.FindProp ("_StandardDetailNormal", props));
               }
               if (mat.HasProperty ("_StandardDetailUVScaleOffset"))
               {
                  materialEditor.ShaderProperty (shaderGUI.FindProp ("_StandardDetailUVScaleOffset", props), CStandardDetailUVScale);
               }
               if (mat.HasProperty ("_StandardDetailNormalScale"))
               {
                  materialEditor.ShaderProperty (shaderGUI.FindProp ("_StandardDetailNormalScale", props), CStandardDetailNormalScale);
               }

               if (!perTexNormalBlend && mat.HasProperty ("_CombinedMeshNormalBlend"))
               {
                  materialEditor.ShaderProperty (shaderGUI.FindProp ("_CombinedMeshNormalBlend", props), "Normal Blend");
               }
#if __MICROSPLAT_TESSELLATION__
               if (mat.HasProperty ("_MeshCombineTessBlend"))
               {
                  materialEditor.ShaderProperty (shaderGUI.FindProp ("_MeshCombineTessBlend", props), CTessellationBlend);
               }
               if (mat.HasProperty ("_MeshCombineTessOffset") && (mat.HasProperty ("_StandardPackedMap") || mat.HasProperty ("_StandardHeight")))
               {
                  Texture2D tex = null;
                  if (mat.HasProperty ("_StandardPackedMap"))
                  {
                     tex = mat.GetTexture ("_StandardPackedMap") as Texture2D;
                  }
                  else
                  {
                     tex = mat.GetTexture ("_StandardHeight") as Texture2D;
                  }
                  if (tex != null)
                  {
                     EditorGUILayout.BeginHorizontal ();
                     materialEditor.ShaderProperty (shaderGUI.FindProp ("_MeshCombineTessOffset", props), CTessellationOffset);
                     if (GUILayout.Button ("Center", GUILayout.Width (54)))
                     {
                        float offset = MicroSplatTessellation.IdealOffset (tex, mat.HasProperty ("_StandardPackedMap") ? 3 : 1);
                        mat.SetFloat ("_MeshCombineTessOffset", offset);
                     }
                     EditorGUILayout.EndHorizontal ();
                  }
               }
#endif
               materialEditor.ShaderProperty (shaderGUI.FindProp ("_StandardUVScaleOffset", props), "UV Scale/Offset");
            }
         }
         if (keywords.IsKeywordEnabled ("_MICROMESH"))
         {
            // TODO: instancing just seems to turn the material black in HDRP, disable for now..
            if (keywords.IsKeywordEnabled ("_MSRENDERLOOP_UNITYHD") || keywords.IsKeywordEnabled("_MSRENDERLOOP_UNITYHDRP2020"))
            {
               mat.enableInstancing = false;
            }
            else
            {
               materialEditor.EnableInstancingField ();
            }
         }
         //materialEditor.DoubleSidedGIField();
      }



      public override void WriteProperties (string [] features, System.Text.StringBuilder sb)
      {
         if (meshWorkflow == DataSource.None)
            return;
         if (meshWorkflow == DataSource.Texture)
         {
            sb.AppendLine ("      _UVMeshRange(\"Mesh UV Range\", Vector) = (0,0,1,1)");
         }

         if (shaderType == ShaderType.Overlay)
         {
            sb.AppendLine ("      _MeshAlphaIndex(\"MeshAlphaIndex\", Int) = 0");
         }
         else if (shaderType == ShaderType.Combined)
         {
            sb.AppendLine ("      _MeshAlphaIndex(\"MeshAlphaIndex\", Int) = 0");

            sb.AppendLine ("      _StandardDiffuse(\"Diffuse/AO\", 2D) = \"white\" { }");
            sb.AppendLine ("      _StandardDiffuseTint(\"Tint\", Color) = (1,1,1,1)");
            sb.AppendLine ("      _StandardNormal(\"Normal\", 2D) = \"bump\" { }");
            sb.AppendLine ("      _CombinedMeshNormalBlend(\"Normal Blend\", Range(0, 1.5)) = 1");

            if (PBRPacking == PBRMapPacking.Packed)
            {
               sb.AppendLine ("      _StandardPackedMap(\"Smoothness / Metal\", 2D) = \"black\" { }");
            }
            else
            {
               if (combinedSmoothMetal)
               {
                  sb.AppendLine ("      _StandardSmoothMetal(\"Smoothness / Metal\", 2D) = \"black\" { }");
               }
               else
               {
                  sb.AppendLine ("      _StandardSmoothness(\"Smoothness\", Range(0, 1)) = 0");
                  sb.AppendLine ("      _StandardMetal(\"Metallic\", Range(0, 1)) = 0");
               }

               if (combinedHeight)
               {
                  sb.AppendLine ("      _StandardHeight(\"Height\", 2D) = \"grey\" { }");
               }

               if (combinedOcclusion)
               {
                  sb.AppendLine ("      _StandardOcclusion(\"Occlusion\", 2D) = \"white\" { }");
               }
               if (HasFeature (features, "_TESSDISTANCE") && (combinedHeight || PBRPacking == PBRMapPacking.Packed))
               {
                  sb.AppendLine ("      _MeshCombineTessBlend(\"Tess Blend\", Range(0, 1)) = 0.5");
                  sb.AppendLine ("      _MeshCombineTessOffset(\"Tess Offset\", Range(-1, 1)) = 0");
               }
            }
            if (combinedSpecular)
            {
               sb.AppendLine ("      _StandardSpecular(\"Specular\", 2D) = \"black\" { }");
            }
            if (combinedEmission)
            {
               sb.AppendLine ("      _StandardEmission(\"Emission\", 2D) = \"black\" { }");
            }
            if (sssMode == SSSMode.Texture)
            {
               sb.AppendLine ("      _StandardSSSMap(\"Subsurface\", 2D) = \"black\" { }");
            }
            else if (sssMode == SSSMode.Value)
            {
               sb.AppendLine ("      _StandardSSSValue(\"Subsurface\", Range(0,1)) = 0.5");
            }
            if (sssMode != SSSMode.None)
            {
               sb.AppendLine ("      _StandardSSSTint(\"Subsurface Tint\", Color) = (1, 1, 1, 1)");
               sb.AppendLine ("      _StandardSSSBlend(\"Subsurface Blend\", Range(0,1)) = 0.5");
            }
            if (combinedDetailAlbedo)
            {
               sb.AppendLine ("      _StandardDetailAlbedo(\"DetailAlbedo\", 2D) = \"white\" {}");
            }
            if (combinedDetailNormal)
            {
               sb.AppendLine ("      _StandardDetailNormal(\"DetailNormal\", 2D) = \"white\" {}");
               sb.AppendLine ("      _StandardDetailNormalScale(\"DetailNormalStrength\", Range(0, 2)) = 1");
            }
            if (combinedDetailNormal || combinedDetailAlbedo)
            {
               sb.AppendLine ("      _StandardDetailUVScaleOffset(\"Detail UV Scale\", Vector) = (5,5,0,0)");
            }


            sb.AppendLine ("      _StandardUVScaleOffset(\"UV Scale and Offset\", Vector) = (1, 1, 0, 0)");

         }
         if (meshWorkflow == DataSource.Texture)
         {
            if (subArray)
            {
               sb.AppendLine ("      _MeshSubArrayIndexes(\"SubArray\", Vector) = (0,1,2,3)");
            }
#if __MICROSPLAT_TESSELLATION__
            if (displacementDampening)
            {
               sb.AppendLine ("      _DisplacementDampening(\"Displacement Dampening\", 2D) = \"white\" {}");
            }
#endif

         }
      }

      public override void WritePerMaterialCBuffer (string[] features, System.Text.StringBuilder sb)
      {
         if (meshWorkflow == DataSource.None)
            return;

         if (meshWorkflow == DataSource.Texture)
         {
            sb.AppendLine ("      float4 _UVMeshRange;");
         }

         if (shaderType != ShaderType.SplatMap)
         {
            sb.AppendLine("      int _MeshAlphaIndex;");
         }
         if (shaderType == ShaderType.Combined)
         {
            sb.AppendLine(combinedCBuffer.text);
         }
      }

      public override void WriteFunctions(string [] features, System.Text.StringBuilder sb)
      {
         if (meshWorkflow != DataSource.None)
         {
            if (meshWorkflow == DataSource.Vertex)
            {
               sb.AppendLine (vertexFunc.text);
            }
            if (shaderType == ShaderType.Combined)
            {
               sb.AppendLine (combinedFunc.text);
            }
         }
      }



      public override void ComputeSampleCounts(string[] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {
         if (meshWorkflow == DataSource.None)
            return;
         if (shaderType == ShaderType.Combined)
         {
            textureSampleCount += 2;
            if (combinedEmission)
               textureSampleCount++;
            if (combinedDetailAlbedo)
               textureSampleCount++;
            if (combinedDetailNormal)
               textureSampleCount++;
            if (combinedSpecular)
               textureSampleCount++;

            if (PBRPacking == PBRMapPacking.Packed)
            {
               textureSampleCount++;
            }
            else
            {
               if (combinedHeight)
                  textureSampleCount++;
               if (combinedSmoothMetal)
                  textureSampleCount++;
               if (combinedOcclusion)
                  textureSampleCount++;
            }
            
            if (sssMode == SSSMode.Texture)
            {
               textureSampleCount++;
            }
            
         }
      }

      public static GUIContent CPerTexNormalBlend = new GUIContent ("Mesh Normal Blend", "Blends between mixed normals and splat map normals");

      public override void DrawPerTextureGUI (int index, MicroSplatKeywords keywords, Material mat, MicroSplatPropData propData)
      {
         if (keywords.IsKeywordEnabled ("_MESHCOMBINED"))
         {
            perTexNormalBlend = DrawPerTexFloatSlider (index, 12, GetFeatureName (DefineFeature._MESHCOMBINEDPERTEXNORMALBLEND),
               keywords, propData, Channel.A, CPerTexNormalBlend, 0.0f, 1.5f);
         }
      }


   }   

#endif
}