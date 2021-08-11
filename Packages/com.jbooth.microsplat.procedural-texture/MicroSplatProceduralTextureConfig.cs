using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBooth.MicroSplat
{

   public class MicroSplatProceduralTextureConfig : ScriptableObject
   {
#if UNITY_EDITOR
      public enum ValueMode
      {
         Scalar,
         EightBit
      }

      public ValueMode valueMode = ValueMode.Scalar;
#endif

      public enum TableSize
      {
         k64 = 64,
         k128 = 128,
         k256 = 256,
         k512 = 512,
         k1024 = 1024,
         k2048 = 2048,
         k4096 = 4096
      }


      public void ResetToDefault()
      {
         layers = new List<Layer>(3);
         layers.Add(new Layer());
         layers.Add(new Layer());
         layers.Add(new Layer());
         layers[1].textureIndex = 1;
         layers[1].slopeActive = true;
         layers[1].slopeCurve = new AnimationCurve(new Keyframe[4] { new Keyframe(0.03f, 0), new Keyframe(0.06f, 1), new Keyframe(0.16f, 1), new Keyframe(0.2f, 0) });
         layers[0].slopeActive = true;
         layers[0].textureIndex = 2;
         layers[0].slopeCurve = new AnimationCurve(new Keyframe[2] { new Keyframe(0.13f, 0), new Keyframe(0.25f, 1) });
      }

      public TableSize proceduralCurveTextureSize = TableSize.k256;

      [System.Serializable]
      public class Layer
      {
         [System.Serializable]
         public class Filter
         {
            public float center = 0.5f;
            public float width = 0.1f;
            public float contrast = 1;
         }


         public float weight = 1;
         public int textureIndex = 0;
         public bool noiseActive;
         public float noiseFrequency = 1;
         public float noiseOffset = 0;
         public Vector2 noiseRange = new Vector2(0, 1);

         public Vector4 biomeWeights = new Vector4(1, 1, 1, 1);
         public Vector4 biomeWeights2 = new Vector4(1, 1, 1, 1);

         public bool heightActive;
         public AnimationCurve heightCurve = AnimationCurve.Linear(0, 1, 1, 1);
         public Filter heightFilter = new Filter();

         public bool slopeActive;
         public AnimationCurve slopeCurve = AnimationCurve.Linear(0, 1, 1, 1);
         public Filter slopeFilter = new Filter();

         public bool erosionMapActive;
         public AnimationCurve erosionMapCurve = AnimationCurve.Linear(0, 1, 1, 1);
         public Filter erosionFilter = new Filter();

         public bool cavityMapActive;
         public AnimationCurve cavityMapCurve = AnimationCurve.Linear(0, 1, 1, 1);
         public Filter cavityMapFilter = new Filter();

         public enum CurveMode
         {
            Curve,
            BoostFilter,
            HighPass,
            LowPass,
            CutFilter
         }

         public CurveMode heightCurveMode = CurveMode.Curve;
         public CurveMode slopeCurveMode = CurveMode.Curve;
         public CurveMode erosionCurveMode = CurveMode.Curve;
         public CurveMode cavityCurveMode = CurveMode.Curve;

#if UNITY_EDITOR
         public bool heightCurveSheet = false;
         public bool slopeCurveSheet = false;
         public bool erosionCurveSheet = false;
         public bool cavityCurveSheet = false;
#endif

         public Layer Copy()
         {
            Layer l = new Layer();
            l.weight = weight;
            l.textureIndex = textureIndex;
            l.noiseActive = noiseActive;
            l.noiseFrequency = noiseFrequency;
            l.noiseOffset = noiseOffset;
            l.noiseRange = noiseRange;
            l.biomeWeights = biomeWeights;
            l.biomeWeights2 = biomeWeights2;
            l.heightActive = heightActive;
            l.slopeActive = slopeActive;
            l.erosionMapActive = erosionMapActive;
            l.cavityMapActive = cavityMapActive;
            l.heightCurve = new AnimationCurve(heightCurve.keys);
            l.slopeCurve = new AnimationCurve(slopeCurve.keys);
            l.erosionMapCurve = new AnimationCurve(erosionMapCurve.keys);
            l.cavityMapCurve = new AnimationCurve(cavityMapCurve.keys);
            l.cavityMapFilter = cavityMapFilter;
            l.heightFilter = heightFilter;
            l.slopeFilter = slopeFilter;
            l.erosionFilter = erosionFilter;
            l.heightCurveMode = heightCurveMode;
            l.slopeCurveMode = slopeCurveMode;
            l.erosionCurveMode = erosionCurveMode;
            l.cavityCurveMode = cavityCurveMode;
            return l;
         }
      }

      [System.Serializable]
      public class HSVCurve
      {
         public AnimationCurve H = AnimationCurve.Linear(0, 0.5f, 1, 0.5f);
         public AnimationCurve S = AnimationCurve.Linear(0, 0.0f, 1, 0.0f);
         public AnimationCurve V = AnimationCurve.Linear(0, 0.0f, 1, 0.0f);
      }


      public List<Gradient> heightGradients = new List<Gradient>();
      public List<HSVCurve> heightHSV = new List<HSVCurve>();
      public List<Gradient> slopeGradients = new List<Gradient>();
      public List<HSVCurve> slopeHSV = new List<HSVCurve>();


      [HideInInspector]
      public List<Layer> layers = new List<Layer>();


      // cached textures, generated on demand when Syncing
      [HideInInspector] public Texture2D curveTex;
      [HideInInspector] public Texture2D paramTex;
      [HideInInspector] public Texture2D heightGradientTex;
      [HideInInspector] public Texture2D heightHSVTex;
      [HideInInspector] public Texture2D slopeGradientTex;
      [HideInInspector] public Texture2D slopeHSVTex;

      public Texture2D FindSubAssetTexture(string name)
      {
#if UNITY_EDITOR
         var subs = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(UnityEditor.AssetDatabase.GetAssetPath(this));
         Debug.Log(subs);
         foreach (var s in subs)
         {
            if (s.name == name)
            {
               Texture2D t = (Texture2D)s;
               if (t != null)
               {
                  return t;
               }
            }
         }
#endif

         return null;
      }

      // 32, 128 LUT for curves. R = Height, G = Slope
      public Texture2D GetHeightGradientTexture()
      {
         int height = MicroSplatPropData.sMaxTextures;  // max layers
         int width = 128;
         if (heightGradientTex == null)
         {
            heightGradientTex = FindSubAssetTexture("heightGradientTex");
         }

         if (heightGradientTex == null)
         {
            heightGradientTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
#if UNITY_EDITOR
            heightGradientTex.name = "heightGradientTex";
            UnityEditor.AssetDatabase.AddObjectToAsset(heightGradientTex, this);
            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(heightGradientTex));
#endif
         }

         Color grey = Color.grey;
         for (int i = 0; i < heightGradients.Count; ++i)
         {
            for (int x = 0; x < width; ++x)
            {
               Color c = grey;
               float v = (float)x / width;
               c = heightGradients[i].Evaluate(v);
               heightGradientTex.SetPixel(x, i, c);
            }
         }
         for (int i = heightGradients.Count; i < MicroSplatPropData.sMaxTextures; ++i)
         {
            for (int x = 0; x < width; ++x)
            {
               heightGradientTex.SetPixel(x, i, grey);
            }
         }


         heightGradientTex.Apply(false, false);
         return heightGradientTex;
      }


      // 32, 128 LUT for HSV curves
      public Texture2D GetHeightHSVTexture()
      {
         int height = MicroSplatPropData.sMaxTextures;  // max layers
         int width = 128;
         if (heightHSVTex == null)
         {
            heightHSVTex = FindSubAssetTexture("heightHSVTex");
         }

         if (heightHSVTex == null)
         {
            heightHSVTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
#if UNITY_EDITOR
            heightHSVTex.name = "heightHSVTex";
            UnityEditor.AssetDatabase.AddObjectToAsset(heightHSVTex, this);
            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(heightHSVTex));
#endif
         }

         Color grey = Color.grey;
         for (int i = 0; i < heightHSV.Count; ++i)
         {
            for (int x = 0; x < width; ++x)
            {
               Color c = grey;
               float v = (float)x / width;
               c.r = heightHSV[i].H.Evaluate(v) * 0.5f + 0.5f;
               c.g = heightHSV[i].S.Evaluate(v) * 0.5f + 0.5f;
               c.b = heightHSV[i].V.Evaluate(v) * 0.5f + 0.5f;
               heightHSVTex.SetPixel(x, i, c);
            }
         }
         for (int i = heightHSV.Count; i < MicroSplatPropData.sMaxTextures; ++i)
         {
            for (int x = 0; x < width; ++x)
            {
               heightHSVTex.SetPixel(x, i, grey);
            }
         }


         heightHSVTex.Apply(false, false);

#if UNITY_EDITOR
         UnityEditor.EditorUtility.SetDirty(heightHSVTex);
#endif

         return heightHSVTex;
      }

      // 32, 128 LUT for curves. R = Height, G = Slope
      public Texture2D GetSlopeGradientTexture()
      {
         int height = MicroSplatPropData.sMaxTextures;  // max layers
         int width = 128;

         if (slopeGradientTex == null)
         {
            slopeGradientTex = FindSubAssetTexture("slopeGradientTex");
         }

         if (slopeGradientTex == null)
         {
            slopeGradientTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
#if UNITY_EDITOR
            slopeGradientTex.name = "slopeGradientTex";
            UnityEditor.AssetDatabase.AddObjectToAsset(slopeGradientTex, this);
            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(slopeGradientTex));
#endif
         }

         Color grey = Color.grey;
         for (int i = 0; i < slopeGradients.Count; ++i)
         {
            for (int x = 0; x < width; ++x)
            {
               Color c = grey;
               float v = (float)x / width;
               c = slopeGradients[i].Evaluate(v);
               slopeGradientTex.SetPixel(x, i, c);
            }
         }
         for (int i = slopeGradients.Count; i < MicroSplatPropData.sMaxTextures; ++i)
         {
            for (int x = 0; x < width; ++x)
            {
               slopeGradientTex.SetPixel(x, i, grey);
            }
         }


         slopeGradientTex.Apply(false, false);

#if UNITY_EDITOR
         UnityEditor.EditorUtility.SetDirty(slopeGradientTex);
#endif
         return slopeGradientTex;
      }


      // 32, 128 LUT for HSV curves
      public Texture2D GetSlopeHSVTexture()
      {
         int height = MicroSplatPropData.sMaxTextures;  // max layers
         int width = 128;

         if (slopeHSVTex == null)
         {
            slopeHSVTex = FindSubAssetTexture("slopeHSVTex");
         }

         if (slopeHSVTex == null)
         {
            slopeHSVTex = new Texture2D(width, height, TextureFormat.RGBA32, false);
#if UNITY_EDITOR
            slopeHSVTex.name = "slopeHSVTex";
            UnityEditor.AssetDatabase.AddObjectToAsset(slopeHSVTex, this);
            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(slopeHSVTex));
#endif
         }

         Color grey = Color.grey;
         for (int i = 0; i < slopeHSV.Count; ++i)
         {
            for (int x = 0; x < width; ++x)
            {
               Color c = grey;
               float v = (float)x / width;
               c.r = slopeHSV[i].H.Evaluate(v) * 0.5f + 0.5f;
               c.g = slopeHSV[i].S.Evaluate(v) * 0.5f + 0.5f;
               c.b = slopeHSV[i].V.Evaluate(v) * 0.5f + 0.5f;
               slopeHSVTex.SetPixel(x, i, c);
            }
         }
         for (int i = slopeHSV.Count; i < MicroSplatPropData.sMaxTextures; ++i)
         {
            for (int x = 0; x < width; ++x)
            {
               slopeHSVTex.SetPixel(x, i, grey);
            }
         }

#if UNITY_EDITOR
         UnityEditor.EditorUtility.SetDirty(slopeHSVTex);
#endif

         slopeHSVTex.Apply(false, false);
         return slopeHSVTex;
      }

      float CompFilter(MicroSplatProceduralTextureConfig.Layer.Filter f, MicroSplatProceduralTextureConfig.Layer.CurveMode mode, float v)
      {
         float pos = Mathf.Abs(v - f.center) * (1.0f / Mathf.Max(f.width, 0.0001f));
         pos = Mathf.Clamp01(Mathf.Pow(pos, f.contrast));

         switch (mode)
         {
            case Layer.CurveMode.BoostFilter:
               return 1.0f - pos;
            case Layer.CurveMode.LowPass:
               return v > f.center ? 1.0f - pos : 1;
            case Layer.CurveMode.HighPass:
               return v < f.center ? 1.0f - pos : 1;
            case Layer.CurveMode.CutFilter:
               return pos;
         }
         Debug.LogError("Unhandled case in ProceduralTextureConfig::CompFilter");
         return 0;
      }

      // 32, tableSize LUT for curves. R = Height, G = Slope, B = Cavity, A = Erosion
      public Texture2D GetCurveTexture()
      {
         int height = MicroSplatPropData.sMaxTextures;  // max layers
         int width = (int)proceduralCurveTextureSize;

         if (curveTex == null)
         {
            curveTex = FindSubAssetTexture("curveTex");
         }

         if (curveTex != null && curveTex.width != width)
         {
            DestroyImmediate(curveTex, true);
            curveTex = null;
         }

         if (curveTex == null)
         {
            curveTex = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
#if UNITY_EDITOR
            curveTex.name = "curveTex";
            UnityEditor.AssetDatabase.AddObjectToAsset(curveTex, this);
            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(curveTex));
#endif
         }


         Color white = Color.white;
         for (int i = 0; i < layers.Count; ++i)
         {
            for (int x = 0; x < width; ++x)
            {
               Color c = white;
               float v = (float)x / width;
               if (layers[i].heightActive)
               {
                  if (layers[i].heightCurveMode == Layer.CurveMode.Curve)
                  {
                     c.r = layers[i].heightCurve.Evaluate(v);
                  }
                  else
                  {
                     c.r = CompFilter(layers[i].heightFilter, layers[i].heightCurveMode, v);
                  }
               }
               if (layers[i].slopeActive)
               {
                  if (layers[i].slopeCurveMode == Layer.CurveMode.Curve)
                  {
                     c.g = layers[i].slopeCurve.Evaluate(v);
                  }
                  else
                  {
                     c.g = CompFilter(layers[i].slopeFilter, layers[i].slopeCurveMode, v);
                  }
               }
               if (layers[i].cavityMapActive)
               {
                  if (layers[i].cavityCurveMode == Layer.CurveMode.Curve)
                  {
                     c.b = layers[i].cavityMapCurve.Evaluate(v);
                  }
                  else
                  {
                     c.b = CompFilter(layers[i].cavityMapFilter, layers[i].cavityCurveMode, v);
                  }
               }
               if (layers[i].erosionMapActive)
               {
                  if (layers[i].erosionCurveMode == Layer.CurveMode.Curve)
                  {
                     c.a = layers[i].erosionMapCurve.Evaluate(v);
                  }
                  else
                  {
                     c.a = CompFilter(layers[i].erosionFilter, layers[i].erosionCurveMode, v);
                  }
               }
               curveTex.SetPixel(x, i, c);
            }
         }


         curveTex.Apply(false, false);
#if UNITY_EDITOR
         UnityEditor.EditorUtility.SetDirty(curveTex);
#endif
         return curveTex;
      }



      // 4x32 LUT, noise aprams in x0 (RGBA), weights in x1.r
      public Texture2D GetParamTexture()
      {
         int height = 32;  // max textures
         int width = 4;
         if (paramTex == null || paramTex.format != TextureFormat.RGBAHalf || paramTex.width != width)
         {
            paramTex = new Texture2D(width, height, TextureFormat.RGBAHalf, false, true);
#if UNITY_EDITOR
            paramTex.name = "paramTex";
            UnityEditor.AssetDatabase.AddObjectToAsset(paramTex, this);
            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(paramTex));
#endif
         }


         Color black = new Color(0, 0, 0, 0);
         for (int i = 0; i < layers.Count; ++i)
         {
            Color c0 = black;
            Color c1 = black;
            if (layers[i].noiseActive)
            {
               c0.r = layers[i].noiseFrequency;
               c0.g = layers[i].noiseRange.x;
               c0.b = layers[i].noiseRange.y;
               c0.a = layers[i].noiseOffset;
            }
            c1.r = layers[i].weight;
            c1.g = layers[i].textureIndex;
            paramTex.SetPixel(0, i, c0);
            paramTex.SetPixel(1, i, c1);
            Vector4 bw = layers[i].biomeWeights;
            paramTex.SetPixel(2, i, new Color(bw.x, bw.y, bw.z, bw.w));
            Vector4 bw2 = layers[i].biomeWeights2;
            paramTex.SetPixel(3, i, new Color(bw2.x, bw2.y, bw2.z, bw2.w));
         }

         paramTex.Apply(false, false);

#if UNITY_EDITOR
         UnityEditor.EditorUtility.SetDirty(paramTex);
#endif
         return paramTex;
      }

   }

}