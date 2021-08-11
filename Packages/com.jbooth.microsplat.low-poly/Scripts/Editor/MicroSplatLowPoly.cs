//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////


using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections.Generic;
using System.Linq;

namespace JBooth.MicroSplat
{
#if __MICROSPLAT__
   [InitializeOnLoad]
   public class MicroSplatLowPoly : FeatureDescriptor
   {
      const string sDefine = "__MICROSPLAT_LOWPOLY__";
      static MicroSplatLowPoly()
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
         return "Low Poly";
      }

      public override string GetHelpPath ()
      {
         return "/MicroSplat/Core/Documentation/MicroSplat - Low Poly.pdf";
      }

      public enum DefineFeature
      {
         _TOONHARDEDGENORMAL,
         _TOONHARDEDGENORMALQUAD,
         _TOONWIREFRAME,
         _TOONWIREQUADS,
         _TOONWIREWORLDSPACE,
         _TOONPOLYEDGE,
         _TOONFLATTEXTURE,
         _TOONFLATTEXTUREQUAD,
         _WIRESATURATIONBRIGHTNESS,
         kNumFeatures,
      };

      public enum EdgeMode
      {
         None,
         Triangle,
         Quads,
         Poly
      }

      public enum WireSpace
      {
         ScreenSpace,
         WorldSpace
      }

      public enum WireColorMode
      {
         Color,
         SaturationBrightness
      }



      public EdgeMode hardNormalEdge = EdgeMode.None;
      public EdgeMode flatTexture = EdgeMode.None;
      public EdgeMode wireframe = EdgeMode.None;
      public WireSpace wireSpace = WireSpace.ScreenSpace;
      public WireColorMode wireColor = WireColorMode.Color;

      public TextAsset properties_hardedge;
      public TextAsset functions_hardedge;
      public TextAsset cbuffer_hardedge;


      GUIContent CHardEdge = new GUIContent("Hard Edge Normals", "Render normals as hard edged quads or triangles, or from actual triangles, which LOD when used on terrain");
      GUIContent CFlatTexture = new GUIContent("Flat Texture", "Texture each face with a single color from the texture");
      GUIContent CTerrainSize = new GUIContent("Terrain Size", "Size of terrain for virtual quads, usually matched to terrain aphamap size, but doesn't have to be");
      GUIContent CWireFrame = new GUIContent("Wireframe Mode", "Draw Wireframe edges on terrain, as either triangles or quads");
      GUIContent CWireSpace = new GUIContent("Wireframe Space", "Can fixed in screenspace, so wires are always the same size on screen, or in world space, where they get smaller in the distance");
      GUIContent CWireColor = new GUIContent("Wireframe Blend", "Color overlay, or adjust the terrain underneith?");
      GUIContent CBrightness = new GUIContent("Wireframe Brightness", "Modify brightness of texture along wireframe edge");
      GUIContent CSaturation = new GUIContent("Wireframe Saturation", "Modify saturation of texture along wireframe edge");

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
         hardNormalEdge = (EdgeMode)EditorGUILayout.EnumPopup(CHardEdge, hardNormalEdge);
         flatTexture = (EdgeMode)EditorGUILayout.EnumPopup(CFlatTexture, flatTexture);
         wireframe = (EdgeMode)EditorGUILayout.EnumPopup(CWireFrame, wireframe);
         if (wireframe != EdgeMode.None)
         {
            EditorGUI.indentLevel++;
            wireSpace = (WireSpace)EditorGUILayout.EnumPopup(CWireSpace, wireSpace);
            wireColor = (WireColorMode)EditorGUILayout.EnumPopup(CWireColor, wireColor);
            EditorGUI.indentLevel--;
         }
      }


      public override void DrawShaderGUI(MicroSplatShaderGUI shaderGUI, MicroSplatKeywords keywords, Material mat, MaterialEditor materialEditor, MaterialProperty[] props)
      {
         if ((hardNormalEdge != EdgeMode.None || wireframe != EdgeMode.None || flatTexture != EdgeMode.None) && MicroSplatUtilities.DrawRollup("Low Poly"))
         {
            if (mat.HasProperty("_ToonTerrainSize"))
            {
               var sizeProp = shaderGUI.FindProp("_ToonTerrainSize", props);
               EditorGUI.BeginChangeCheck();
               Vector2 v2 = sizeProp.vectorValue;
               v2 = EditorGUILayout.Vector2Field(CTerrainSize, v2);
               if (v2.x <= 0)
               {
                  v2.x = 1;
               }
               if (v2.y < 0)
               {
                  v2.y = 1;
               }
               if (EditorGUI.EndChangeCheck())
               {
                  sizeProp.vectorValue = v2;
               }
            }

            if (hardNormalEdge != EdgeMode.None && mat.HasProperty("_ToonEdgeHardness"))
            {
               materialEditor.RangeProperty(shaderGUI.FindProp("_ToonEdgeHardness", props), "Edge Hardness");
            }
            if (wireframe != EdgeMode.None && mat.HasProperty("_ToonWireSmoothing"))
            {
               if (mat.HasProperty("_ToonWireColor"))
               {
                  materialEditor.ColorProperty(shaderGUI.FindProp("_ToonWireColor", props), "Wire Color");
               }

               materialEditor.RangeProperty(shaderGUI.FindProp("_ToonWireSmoothing", props), "Wire Smoothness");
               materialEditor.RangeProperty(shaderGUI.FindProp("_ToonWireThickness", props), "Wire Thickness");

               if (mat.HasProperty("_ToonWireSaturationBrightness"))
               {
                  var prop = shaderGUI.FindProp("_ToonWireSaturationBrightness", props);
                  EditorGUI.BeginChangeCheck();
                  Vector2 v2 = prop.vectorValue;
                  v2.x = EditorGUILayout.Slider(CSaturation, v2.x, 0, 2);
                  v2.y = EditorGUILayout.Slider(CBrightness, v2.y, 0, 2);

                  if (EditorGUI.EndChangeCheck())
                  {
                     prop.vectorValue = v2;
                  }
               }

            }
         }
      }

      public override void InitCompiler(string[] paths)
      {
         for (int i = 0; i < paths.Length; ++i)
         {
            string p = paths[i];
            if (p.EndsWith("microsplat_func_edges.txt"))
            {
               functions_hardedge = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
            if (p.EndsWith ("microsplat_cbuffer_edges.txt"))
            {
               cbuffer_hardedge = AssetDatabase.LoadAssetAtPath<TextAsset> (p);
            }
            if (p.EndsWith("microsplat_properties_edges.txt"))
            {
               properties_hardedge = AssetDatabase.LoadAssetAtPath<TextAsset>(p);
            }
         }
      } 

      public override void WriteProperties(string[] features, System.Text.StringBuilder sb)
      {
         if (hardNormalEdge == EdgeMode.Quads || hardNormalEdge == EdgeMode.Triangle || wireframe != EdgeMode.None || flatTexture != EdgeMode.None)
         {
            sb.AppendLine("      _ToonTerrainSize(\"Toon Terrain Size\", Vector) = (512,512,0,0)");
         }

         if (hardNormalEdge != EdgeMode.None)
         {
            sb.AppendLine(properties_hardedge.text);
         }
         if (wireframe != EdgeMode.None)
         {
            if (wireColor == WireColorMode.Color)
            {
               sb.AppendLine("      _ToonWireColor(\"Wire Color\", Color) = (0,0,0,1)");
            }
            else
            {
               sb.AppendLine("      _ToonWireSaturationBrightness(\"Wire Params\", Vector) = (1,1,0,0)");
            }
            sb.AppendLine("      _ToonWireSmoothing(\"Wire Smooth\", Range(0.01, 10)) = 1");
            sb.AppendLine("      _ToonWireThickness(\"Wire Thickness\", Range(0.01, 10)) = 1");
         }
      }

      public override void ComputeSampleCounts(string[] features, ref int arraySampleCount, ref int textureSampleCount, ref int maxSamples, ref int tessellationSamples, ref int depTexReadLevel)
      {

      }

      public override string[] Pack()
      {
         List<string> features = new List<string>();
         if (hardNormalEdge != EdgeMode.None)
         {
            if (hardNormalEdge == EdgeMode.Poly)
            {
               features.Add (GetFeatureName (DefineFeature._TOONPOLYEDGE));
            }
            else
            {
               features.Add (GetFeatureName (DefineFeature._TOONHARDEDGENORMAL));
               if (hardNormalEdge == EdgeMode.Quads)
               {
                  features.Add (GetFeatureName (DefineFeature._TOONHARDEDGENORMALQUAD));
               }
            }
         }
         if (wireframe != EdgeMode.None)
         {
            features.Add(GetFeatureName(DefineFeature._TOONWIREFRAME));
            if (wireframe == EdgeMode.Quads)
            {
               features.Add(GetFeatureName(DefineFeature._TOONWIREQUADS));
            }
            if (wireSpace == WireSpace.WorldSpace)
            {
               features.Add(GetFeatureName(DefineFeature._TOONWIREWORLDSPACE));
            }
            if (wireColor == WireColorMode.SaturationBrightness)
            {
               features.Add(GetFeatureName(DefineFeature._WIRESATURATIONBRIGHTNESS));
            }
         }

         if (flatTexture != EdgeMode.None)
         {
            features.Add(GetFeatureName(DefineFeature._TOONFLATTEXTURE));
            if (flatTexture == EdgeMode.Quads)
            {
               features.Add(GetFeatureName(DefineFeature._TOONFLATTEXTUREQUAD));
            }
         }
         return features.ToArray();
      }

      public override void WritePerMaterialCBuffer (string[] features, System.Text.StringBuilder sb)
      {
         if (hardNormalEdge != EdgeMode.None || wireframe != EdgeMode.None)
         {
            sb.AppendLine(cbuffer_hardedge.text);
         }
         if (hardNormalEdge == EdgeMode.Quads || hardNormalEdge == EdgeMode.Triangle || wireframe != EdgeMode.None || flatTexture != EdgeMode.None)
         {
            sb.AppendLine("      float2 _ToonTerrainSize;");
         }
      }

      public override void WriteFunctions(string [] features, System.Text.StringBuilder sb)
      {
         if (hardNormalEdge != EdgeMode.None || wireframe != EdgeMode.None)
         {
            sb.AppendLine(functions_hardedge.text);
         }
      }

      public override void Unpack(string[] keywords)
      {
         hardNormalEdge = HasFeature(keywords, DefineFeature._TOONHARDEDGENORMAL) ? EdgeMode.Triangle : EdgeMode.None;
         if (hardNormalEdge != EdgeMode.None && HasFeature(keywords, DefineFeature._TOONHARDEDGENORMALQUAD))
         {
            hardNormalEdge = EdgeMode.Quads;
         }
         if (HasFeature(keywords, DefineFeature._TOONPOLYEDGE))
         {
            hardNormalEdge = EdgeMode.Poly;
         }
         
         wireframe = HasFeature(keywords, DefineFeature._TOONWIREFRAME) ? EdgeMode.Triangle : EdgeMode.None;
         if (wireframe == EdgeMode.Triangle && HasFeature(keywords, DefineFeature._TOONWIREQUADS))
         {
            wireframe = EdgeMode.Quads;
         }
         if (wireframe != EdgeMode.None)
         {
            wireSpace = WireSpace.ScreenSpace;
            if (HasFeature(keywords, DefineFeature._TOONWIREWORLDSPACE))
               wireSpace = WireSpace.WorldSpace;
         }
         wireColor = HasFeature(keywords, DefineFeature._WIRESATURATIONBRIGHTNESS) ? WireColorMode.SaturationBrightness : WireColorMode.Color;
         flatTexture = HasFeature(keywords, DefineFeature._TOONFLATTEXTURE) ? EdgeMode.Triangle : EdgeMode.None;
         if (flatTexture != EdgeMode.None && HasFeature(keywords, DefineFeature._TOONFLATTEXTUREQUAD))
         {
            flatTexture = EdgeMode.Quads;
         }
      }
   }   
#endif

}