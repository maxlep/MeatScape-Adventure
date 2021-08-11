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
   public class MicroSplatObjectShaderModule : FeatureDescriptor
   {
      const string sDefine = "__MICROSPLAT_OBJECTSHADER__";
      static MicroSplatObjectShaderModule()
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
         return -998;
      }

      public override string ModuleName()
      {
         return "Object Shader";
      }

      public override string GetHelpPath ()
      {
         return "/MicroSplat/Core/Documentation/MicroSplat - Object Shader.pdf";
      }

      public enum DefineFeature
      {
         _OBJECTSHADER,
         kNumFeatures,
      }
         

      public bool exportObjectShader;   
      
      GUIContent CObjectShader = new GUIContent("Export Object Shader", "Exports a custom shader to use with simple meshes in the scene, which will recieve various MicroSplat effects like snow, wetness, etc");


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


      public override void DrawFeatureGUI(MicroSplatKeywords keywords)
      {
         exportObjectShader = EditorGUILayout.Toggle(CObjectShader, exportObjectShader);
      }
         
      static TextAsset combinedFunc;
      static TextAsset combinedCBuffer;

      public override string[] Pack()
      {
         List<string> features = new List<string>();
         if (exportObjectShader)
         {
         	features.Add(GetFeatureName(DefineFeature._OBJECTSHADER));
         }
         
         return features.ToArray();
      }

      public override void Unpack(string[] keywords)
      {
      	 exportObjectShader = HasFeature(keywords, GetFeatureName(DefineFeature._OBJECTSHADER));
      }

      public override void DrawShaderGUI (MicroSplatShaderGUI shaderGUI, MicroSplatKeywords keywords, Material mat, MaterialEditor materialEditor, MaterialProperty [] props)
      {

      }



      public override void WriteProperties (string [] features, System.Text.StringBuilder sb)
      {
         if (exportObjectShader && System.Array.Exists(features, e => e == "_ISOBJECTSHADER"))
         {
            sb.AppendLine ("      _ObjectShaderDiffuse(\"Diffuse/AO\", 2D) = \"white\" { }");
            sb.AppendLine ("      _ObjectShaderAlphaClipThreshold(\"Alpha Threshold\", Range(0,1)) = 0.5");
            sb.AppendLine ("      _ObjectShaderDiffuseTint(\"Tint\", Color) = (1,1,1,1)");
            sb.AppendLine ("      _ObjectShaderNormal(\"Normal\", 2D) = \"bump\" { }");
            sb.AppendLine ("      _ObjectShaderPackedMap(\"Smoothness / Metal\", 2D) = \"black\" { }");
            sb.AppendLine ("      _ObjectShaderEmission(\"Emission\", 2D) = \"black\" { }");
            sb.AppendLine ("      _ObjectShaderDetailNormal(\"DetailNormal\", 2D) = \"bump\" {}");
            sb.AppendLine ("      _ObjectShaderDetailNormalScale(\"DetailNormalStrength\", Range(0, 2)) = 1");
            sb.AppendLine ("      _ObjectShaderDetailUVScaleOffset(\"Detail UV Scale\", Vector) = (5,5,0,0)");
            sb.AppendLine ("      _ObjectShaderUVScaleOffset(\"UV Scale and Offset\", Vector) = (1, 1, 0, 0)");
            sb.AppendLine ("      _ObjectShaderNormalScale(\"Normal Scale\", Range(0, 3)) = 1");
            sb.AppendLine ("      _ObjectShaderMetallic(\"Metallic\", Range(0, 1)) = 0");
            sb.AppendLine ("      _ObjectShaderSmoothness(\"Smoothness\", Range(0, 1)) = 0");
            sb.AppendLine ("      _ObjectShaderDetailAlbedoStrength(\"Detail Albedo Strength\", Range(0,2)) = 1");
            sb.AppendLine ("      _ObjectShaderDetailSmoothnessStrength(\"Detail Smoothness Strength\", Range(0,2)) = 1");
            sb.AppendLine ("      _ObjectShaderSpecular(\"Specular\", 2D) = \"black\" { }");
         }
      }

      public override void WritePerMaterialCBuffer (string[] features, System.Text.StringBuilder sb)
      {
         if (exportObjectShader && System.Array.Exists (features, e => e == "_ISOBJECTSHADER"))
         {
            sb.AppendLine (combinedCBuffer.text);
         }
      }

      public override void WriteFunctions(string[] features, System.Text.StringBuilder sb)
      {
         if (exportObjectShader && System.Array.Exists (features, e => e == "_ISOBJECTSHADER"))
         {
            sb.AppendLine (combinedFunc.text);
         }
      }



      public override void ComputeSampleCounts(string[] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {
         
      }


      public override void DrawPerTextureGUI (int index, MicroSplatKeywords keywords, Material mat, MicroSplatPropData propData)
      {

      }

      public override void InitCompiler (string [] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths [i];
            if (p.EndsWith ("microsplat_objectshader_func.txt"))
            {
               combinedFunc = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
            if (p.EndsWith ("microsplat_objectshader_cbuffer.txt"))
            {
               combinedCBuffer = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
         }
      }

      public override MicroSplatShaderGUI.MicroSplatCompiler.AuxShader GetAuxShader ()
      {
         var aux = new MicroSplatShaderGUI.MicroSplatCompiler.AuxShader ("_OBJECTSHADER", "_objects");
         aux.customEditor = "MicroSplatShaderObjectGUI";
         aux.extension = "_objects";
         return aux;
      }

      public static void ModifyKeywords(List<string> keywords)
      {
         keywords.Add ("_ISOBJECTSHADER");
         keywords.Add ("_DISABLESPLATMAPS");
         keywords.Add ("_MICROMESH");
         keywords.Remove ("_MICROTERRAIN");
         keywords.Remove ("_TESSDISTANCE");
         keywords.Remove ("_MICROVERTEXMESH");
         keywords.Remove ("_MEGASPLAT");
         keywords.Remove ("_MICRODIGGERMESH");

      }

      public override void ModifyKeywordsForAuxShader (List<string> keywords)
      {
         ModifyKeywords (keywords);
      }

      public override void OnPostGeneration (ref StringBuilder sb, string [] features, string name, string baseName = null, MicroSplatShaderGUI.MicroSplatCompiler.AuxShader auxShader = null)
      {
         if (auxShader != null && auxShader.trigger == "_OBJECTSHADER")
         {
            string feature = "#pragma shader_feature ";
#if UNITY_2019_4_OR_NEWER
             feature = "#pragma shader_feature_local ";
#endif

            sb = sb.Replace ("MicroSplatShaderGUI", "MicroSplatShaderObjectGUI");
            sb = sb.Replace ("Geometry+100", "Geometry");

            StringBuilder pragmas = new StringBuilder ();
            pragmas.AppendLine ("\n#define _MICROSPLAT 1");
            pragmas.AppendLine (feature + "_ _OBJECTSHADERALPHACLIP");
            pragmas.AppendLine (feature + "_ _OBJECTSHADERPACKEDMAP");
            pragmas.AppendLine (feature + "_ _OBJECTSHADEREMISSION");
            pragmas.AppendLine (feature + "_ _OBJECTSHADERDETAILPACKED");

            sb.Replace ("#define _MICROSPLAT 1", pragmas.ToString ());
            
         }
      }
   }   

#endif
         }