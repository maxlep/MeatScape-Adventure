using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBooth.MicroSplat
{
   public class SnowUtilities
   {

      static float SnowFade(float worldHeight, float snowMin, float snowMax, float snowDot, float snowDotVertex, float snowLevel, float angleRangeZ, float angleRangeW)
      {
         float snowHeightFade = Mathf.Clamp01((worldHeight - snowMin) / Mathf.Max(snowMax, 0.001f));
         float snowAngleFade = Mathf.Max(0, (snowDotVertex - angleRangeZ) * 6);
         snowAngleFade = snowAngleFade * (1 - Mathf.Max(0, (snowDotVertex - angleRangeW) * 6));
         return Mathf.Clamp01((snowLevel * snowHeightFade * snowAngleFade));
      }




      public static float GetSnowCoverage(Terrain t, Vector3 worldPos, int maxDistance = 2)
      {
         MicroSplatObject mso = t.GetComponent<MicroSplatTerrain>();
         if (mso != null)
         {
            if (mso.keywordSO.IsKeywordEnabled("_SNOW"))
            {
               var terrainLocalPos = worldPos - t.transform.position;
               var normalizedPos = new Vector2(Mathf.InverseLerp(0.0f, t.terrainData.size.x, terrainLocalPos.x),
                                           Mathf.InverseLerp(0.0f, t.terrainData.size.z, terrainLocalPos.z));

               Vector3 worldNormal = t.terrainData.GetInterpolatedNormal(normalizedPos.x, normalizedPos.y);
               float worldheight = t.terrainData.GetInterpolatedHeight(normalizedPos.x, normalizedPos.y);

               float snowLevel;
               var mat = mso.templateMaterial;
               float snowMin;
               float snowMax;
               float angleRangeW;
               float angleRangeZ;

               if (mso.keywordSO.IsKeywordEnabled("_USEGLOBALSNOWLEVEL"))
               {
                  snowLevel = Shader.GetGlobalFloat("_Global_SnowLevel");
               }
               else
               {
                  snowLevel = mat.GetFloat("_SnowAmount");
               }

               if (mso.keywordSO.IsKeywordEnabled("_USEGLOBALSNOWHEIGHT"))
               {
                  var g = Shader.GetGlobalVector("_Global_SnowMinMaxHeight");
                  snowMin = g.x;
                  snowMax = g.y;
                  angleRangeZ = g.z;
                  angleRangeW = g.w;
               }
               else
               {
                  var g = mat.GetVector("_SnowHeightAngleRange");
                  snowMin = g.x;
                  snowMax = g.y;
                  angleRangeZ = g.z;
                  angleRangeW = g.w;
               }

               Vector4 _SnowParams = mat.GetVector("_SnowParams");
               Vector3 snowUpVector = mat.GetVector("_SnowUpVector");


               float snowDot = Mathf.Max(snowLevel / 2, Vector3.Dot(worldNormal, snowUpVector));
               float snowDotVertex = snowDot;
               float ao = 1;
               float oheight = 0;

               float snowFade = SnowFade(worldheight, snowMin, snowMax, snowDot, snowDotVertex, snowLevel, angleRangeZ, angleRangeW);

               float height = Mathf.Clamp01(oheight - (1.0f - _SnowParams.x));
               float erosion = Mathf.Clamp01(ao * _SnowParams.y);
               erosion *= erosion;
               float snowMask = Mathf.Clamp01(snowFade - erosion - height);
               snowMask = snowMask * snowMask * snowMask;
               float snowAmount = snowMask * Mathf.Clamp01(snowDot - (height + erosion) * 0.5f);  // up
               snowAmount = Mathf.Clamp01(snowAmount * 8);


               return snowAmount;
            }
         }

         return 0;

      }

      public static float GetSnowCoverage(Vector3 worldPos, int maxDistance = 2)
      {
         Terrain t = null;
         RaycastHit[] hits = Physics.RaycastAll(worldPos + Vector3.up * 1, Vector3.down, maxDistance + 1);
         for (int i = 0; i < hits.Length; ++i)
         {
            var h = hits[i];
            t = h.collider.GetComponent<Terrain>();
            if (t != null)
            {
               var nt = t.GetComponent<MicroSplatTerrain>();
               if (nt != null)
               {
                  return GetSnowCoverage(t, worldPos, maxDistance);
               }
            }

         }

         return 0;
      }

   }
}
