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
   public class MicroSplatMeshTerrainModule : FeatureDescriptor
   {
      const string sDefine = "__MICROSPLAT_MESHTERRAIN__";
      static MicroSplatMeshTerrainModule()
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
         return "MeshTerrain";
      }

      public enum DefineFeature
      {
         kNumFeatures,
      }

      public override bool HideModule ()
      {
         return true;
      }

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

      public override string GetVersion()
      {
         return "3.8";
      }


      public override void DrawFeatureGUI(MicroSplatKeywords keywords)
      {
      }
         
      static TextAsset combinedFunc;

      public override string[] Pack()
      {
         List<string> features = new List<string>();
         return features.ToArray();
      }

      public override void Unpack(string[] keywords)
      {

      }

      public override void InitCompiler(string[] paths)
      {

      }
         

      public override void DrawShaderGUI(MicroSplatShaderGUI shaderGUI, MicroSplatKeywords keywords, Material mat, MaterialEditor materialEditor, MaterialProperty[] props)
      {

      }



      public override void WriteProperties(string[] features, System.Text.StringBuilder sb)
      {

      }

      public override void WriteFunctions(string [] features, System.Text.StringBuilder sb)
      {

      }

      public override void ComputeSampleCounts(string[] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {

         
      }


   }   

#endif
}