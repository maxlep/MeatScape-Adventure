using UnityEngine;
using System.Collections;

// for sampling texturing from the CPU. If your using jobs, there's a jobified version
// included in a zip file next to this version.

namespace JBooth.MicroSplat
{
   public class MicroSplatProceduralTextureUtil
   {


      public enum NoiseUVMode
      {
         UV = 0,
         World,
         Triplanar
      }

      static float PCFilter(int index, float height, float slope, float cavity, float flow, Vector3 worldPos, Vector2 uv,
          Color bMask, Color bMask2, out int texIndex, Vector3 pN, MicroSplatProceduralTextureConfig config, 
          Texture2D procTexNoise, NoiseUVMode noiseMode)
      {
         var layer = config.layers [index];
         Vector2 noiseUV = uv;

         Color noise = new Color (0, 0, 0, 0);
         if (noiseMode == NoiseUVMode.Triplanar)
         {
            Vector2 nUV0 = new Vector2 (worldPos.z, worldPos.y) * 0.002f * layer.noiseFrequency + new Vector2 (layer.noiseOffset, layer.noiseOffset);
            Vector2 nUV1 = new Vector2 (worldPos.x, worldPos.z) * 0.002f * layer.noiseFrequency + new Vector2 (layer.noiseOffset + 0.31f, layer.noiseOffset + 0.31f);
            Vector2 nUV2 = new Vector2 (worldPos.x, worldPos.y) * 0.002f * layer.noiseFrequency + new Vector2 (layer.noiseOffset + 0.71f, layer.noiseOffset + 0.71f);

            Color noise0 = procTexNoise.GetPixelBilinear (nUV0.x, nUV0.y);
            Color noise1 = procTexNoise.GetPixelBilinear (nUV1.x, nUV1.y);
            Color noise2 = procTexNoise.GetPixelBilinear (nUV2.x, nUV2.y);
            noise = noise0 * pN.x + noise1 * pN.y + noise2 * pN.z;
         }
         else if (noiseMode == NoiseUVMode.World)
         {
            noise = procTexNoise.GetPixelBilinear (noiseUV.x * layer.noiseFrequency + layer.noiseOffset, noiseUV.y * layer.noiseFrequency + layer.noiseOffset);
         }
         else if (noiseMode == NoiseUVMode.UV)
         {
            noise *= procTexNoise.GetPixelBilinear (worldPos.x * 0.002f * layer.noiseFrequency + layer.noiseOffset, worldPos.z * 0.002f * layer.noiseFrequency + layer.noiseOffset);
         }

         // unpack
         noise.r = noise.r * 2 - 1;
         noise.g = noise.g * 2 - 1;


         
         float h0 = layer.heightCurve.Evaluate (height);
         float s0 = layer.slopeCurve.Evaluate (slope);
         float c0 = layer.cavityMapCurve.Evaluate (cavity);
         float f0 = layer.erosionMapCurve.Evaluate (flow);

         h0 *= 1.0f + Mathf.Lerp (layer.noiseRange.x, layer.noiseRange.y, noise.r);
         s0 *= 1.0f + Mathf.Lerp (layer.noiseRange.x, layer.noiseRange.y, noise.g);
         c0 *= 1.0f + Mathf.Lerp (layer.noiseRange.x, layer.noiseRange.y, noise.b);
         f0 *= 1.0f + Mathf.Lerp (layer.noiseRange.x, layer.noiseRange.y, noise.a);

         if (!layer.heightActive)
            h0 = 1;
         if (!layer.slopeActive)
            s0 = 1;
         if (!layer.cavityMapActive)
            c0 = 1;
         if (!layer.erosionMapActive)
            f0 = 1;

         
         bMask *= layer.biomeWeights;
         bMask2 *= layer.biomeWeights2;
         // handle 16 mode?
         float bWeight = Mathf.Max(Mathf.Max(Mathf.Max(bMask.r, bMask.g), bMask.b), bMask.a);
         float bWeight2 = Mathf.Max (Mathf.Max (Mathf.Max (bMask2.r, bMask2.g), bMask2.b), bMask2.a);
         texIndex = layer.textureIndex;
         return Mathf.Clamp01(h0 * s0 * c0 * f0 * layer.weight * bWeight * bWeight2);
      }

      public struct Int4
      {
         public int x;
         public int y;
         public int z;
         public int w;
      }

      static void PCProcessLayer(ref Vector4 weights, ref Int4 indexes, ref float totalWeight,
          int curIdx, float height, float slope, float cavity, float flow, Vector3 worldPos, Vector2 uv,
          Color biomeMask, Color biomeMask2, Vector3 pN,
          MicroSplatProceduralTextureConfig config, 
          Texture2D noiseMap, NoiseUVMode noiseMode)
      {
         int texIndex = 0;
         float w = PCFilter(curIdx, height, slope, cavity, flow, worldPos, uv, biomeMask, biomeMask2, out texIndex, pN, config, noiseMap, noiseMode);
         w = Mathf.Min(totalWeight, w);
         totalWeight -= w;

         // sort
         if (w > weights.x)
         {
            weights.w = weights.z;
            weights.z = weights.y;
            weights.y = weights.x;
            indexes.w = indexes.z;
            indexes.z = indexes.y;
            indexes.y = indexes.x;
            weights.x = w;
            indexes.x = texIndex;
         }
         else if (w > weights.y)
         {
            weights.w = weights.z;
            weights.z = weights.y;
            indexes.w = indexes.z;
            indexes.z = indexes.y;
            weights.y = w;
            indexes.y = texIndex;
         }
         else if (w > weights.z)
         {
            weights.w = weights.z;
            indexes.w = indexes.z;
            weights.z = w;
            indexes.z = texIndex;
         }
         else if (w > weights.w)
         {
            weights.w = w;
            indexes.w = texIndex;
         }
      }

      public static void Sample(
         Vector2 uv,
         Vector3 worldPos,
         Vector3 worldNormal,
         Vector3 up,
         NoiseUVMode noiseUVMode,
         Material mat,
         MicroSplatProceduralTextureConfig config,
         out Vector4 weights,
         out Int4 indexes)
      {
         weights = new Vector4(0, 0, 0, 0);

         int layerCount = config.layers.Count;
         Vector2 worldHeightRange = mat.GetVector ("_WorldHeightRange");
         float worldheight = worldPos.y;

         float height = Mathf.Clamp01((worldheight - worldHeightRange.x) / Mathf.Max(0.1f, (worldHeightRange.y - worldHeightRange.x)));
         float slope = 1.0f - Mathf.Clamp01(Vector3.Dot(worldNormal, up) * 0.5f + 0.49f);
         float cavity = 0.5f;
         float flow = 0.5f;

         var cavityMap = mat.HasProperty ("_CavityMap") ? (Texture2D)mat.GetTexture ("_CavityMap") : null;

         if (cavityMap != null)
         {
            var p = cavityMap.GetPixelBilinear(uv.x, uv.y);
            cavity = p.g;
            flow = p.a;
         }
         // find 4 highest weights and indexes
         indexes = new Int4();
         indexes.x = 0;
         indexes.y = 1;
         indexes.z = 2;
         indexes.w = 3;

         float totalWeight = 1.0f;

         var mask = mat.HasProperty ("_ProcTexBiomeMask") ? (Texture2D)mat.GetTexture ("_ProcTexBiomeMask") : null;
         var mask2 = mat.HasProperty ("_ProcTexBiomeMask2") ? (Texture2D)mat.GetTexture ("_ProcTexBiomeMask2") : null;


         Color biomeMask = new Color(1, 1, 1, 1);
         Color biomeMask2 = new Color (1, 1, 1, 1);
         if (mask != null)
         {
            biomeMask = mask.GetPixelBilinear(uv.x, uv.y);
         }
         if (mask2 != null)
         {
            biomeMask2 = mask.GetPixelBilinear(uv.x, uv.y);
         }

         Vector3 pN = new Vector3(0, 0, 0);
         if (noiseUVMode == NoiseUVMode.Triplanar)
         {
            Vector3 absWN = worldNormal;
            absWN.x = Mathf.Abs(absWN.x);
            absWN.y = Mathf.Abs(absWN.y);
            absWN.z = Mathf.Abs(absWN.z);
            pN.x = Mathf.Pow(absWN.x, 4);
            pN.y = Mathf.Pow(absWN.y, 4);
            pN.z = Mathf.Pow(absWN.z, 4);
            float ttl = pN.x + pN.y + pN.z;
            pN.x /= ttl; 
            pN.y /= ttl;
            pN.z /= ttl;
         }

         var noiseMap = mat.HasProperty ("_ProcTexNoise") ? (Texture2D)mat.GetTexture ("_ProcTexNoise") : null;



         for (int i = 0; i < layerCount; ++i)
         {
            PCProcessLayer(ref weights, ref indexes, ref totalWeight, i, height, slope, cavity, flow, worldPos, uv, biomeMask, biomeMask2, pN, config, noiseMap, noiseUVMode);
            if (totalWeight <= 0)
               break;
         }

      }
   }
}

