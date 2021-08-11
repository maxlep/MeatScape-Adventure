//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBooth.MicroSplat
{
   public partial class MicroSplatDecalReceiver : MonoBehaviour
   {
      [HideInInspector]
      public List<MicroSplatDecal> dynamicDecals = null;
      [HideInInspector]
      public Texture2D dynamicCacheData;
      [HideInInspector]
      public Texture2D dynamicCullData;

      int maxDynamicDecals = 0;

      public int dynamicCount { get { return dynamicDecals == null ? 0 : dynamicDecals.Count; } }

      void ClearDynamicCacheData()
      {
         if (staticCacheData != null)
         {
            DestroyImmediate(dynamicCacheData);
         }
         if (dynamicCullData != null)
         {
            DestroyImmediate(dynamicCullData);
         }
         dynamicCacheData = new Texture2D(maxDynamicDecals, 8, TextureFormat.RGBAFloat, false, true);
         dynamicCacheData.Apply(false, false);
         dynamicCullData = new Texture2D(maxDynamicDecals, 1, TextureFormat.RGBAFloat, false, true);
         dynamicCullData.Apply(false, false);
         dynamicCacheData.hideFlags = HideFlags.HideAndDontSave;
         dynamicCullData.hideFlags = HideFlags.HideAndDontSave;
      }

      private void InitDynamic()
      {
         maxDynamicDecals = 8;
         if (msObj.keywordSO.IsKeywordEnabled("_DECAL_MAX0"))
         {
            maxDynamicDecals = 1;
         }
         if (msObj.keywordSO.IsKeywordEnabled("_DECAL_MAX16"))
         {
            maxDynamicDecals = 16;
         }
         else if (msObj.keywordSO.IsKeywordEnabled("_DECAL_MAX32"))
         {
            maxDynamicDecals = 32;
         }
         else if (msObj.keywordSO.IsKeywordEnabled("_DECAL_MAX64"))
         {
            maxDynamicDecals = 64;
         }
         else if (msObj.keywordSO.IsKeywordEnabled("_DECAL_MAX128"))
         {
            maxDynamicDecals = 128;
         }
         else if (msObj.keywordSO.IsKeywordEnabled("_DECAL_MAX256"))
         {
            maxDynamicDecals = 256;
         }

         dynamicDecals = new List<MicroSplatDecal>(maxDynamicDecals);
         ClearDynamicCacheData();
      }

      void RegisterDynamicDecal(MicroSplatDecal d)
      {
         bool contains = staticDecals.Contains(d);
         if (!contains)
         {
            dynamicDecals.Add(d);
            if (dynamicDecals.Count > 1 && d.sortOrder != dynamicDecals[dynamicDecals.Count - 2].sortOrder)
            {
               dynamicDecals.Sort((x, y) => x.sortOrder.CompareTo(y.sortOrder));
            }
         }
      }

      void UpdateDynamicPropertyBlocks()
      {
         int count = dynamicDecals.Count;
         if (count > maxDynamicDecals)
            count = maxDynamicDecals;

         for (int i = 0; i < count; ++i)
         {
            Vector3 scale = dynamicDecals[i].transform.lossyScale - Vector3.zero;
            float cullDist = scale.sqrMagnitude * 0.5f;

            Vector3 pos = dynamicDecals[i].transform.position;
            dynamicCullData.SetPixel(i, 0, new Color(pos.x, pos.y, pos.z, cullDist));

            SetData(dynamicDecals[i], i, dynamicCacheData);
         }
         dynamicCacheData.Apply(false, false);
         dynamicCullData.Apply(false, false);
         decalBlock.SetInt("_MSDecalCount", count);
         decalBlock.SetTexture("_DecalCullData", dynamicCullData);
         decalBlock.SetTexture("_DecalDynamicData", dynamicCacheData);

      }

      void UnregisterDynamicDecal(MicroSplatDecal d)
      {
         if (dynamicDecals != null && dynamicDecals.Contains(d))
         {
            dynamicDecals.Remove(d);
         }
      }

   }
}
