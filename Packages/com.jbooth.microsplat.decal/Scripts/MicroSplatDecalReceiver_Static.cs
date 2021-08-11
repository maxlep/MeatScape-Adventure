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
      List<MicroSplatDecal> staticDecals = null;

      [HideInInspector]
      public Texture2D cacheMask = null; // holds pre-culled indexes
      [HideInInspector]
      public Color[] cacheMaskBuffer = null;
      [HideInInspector]
      public Texture2D staticCacheData = null; // holds decal data, because uploading texture is fast, property block arrays not so much



      public int staticCount { get { return staticDecals == null ? 0 : staticDecals.Count; } }

      int maxStaticDecals = 0;

      public enum StaticCacheSize
      {
         k64 = 64,
         k128 = 128,
         k256 = 256,
         k512 = 512,
         k1024 = 1024
      }
      public StaticCacheSize staticCacheSize = StaticCacheSize.k256;

      void ClearStaticCacheData()
      {
         if (staticCacheData != null)
         {
            DestroyImmediate(staticCacheData);
         }
         staticCacheData = new Texture2D(maxStaticDecals, 8, TextureFormat.RGBAFloat, false, true);
         staticCacheData.Apply(false, false);
         staticCacheData.hideFlags = HideFlags.HideAndDontSave;
      }


      void ClearCacheMask()
      {
         if (cacheMask != null)
         {
            DestroyImmediate(cacheMask);
         }

         // setup cache mask
         int size = (int)staticCacheSize;
         if (cacheMaskBuffer == null || cacheMaskBuffer.Length != size * size)
         {
            ClearCacheMaskBuffer();
         }
         cacheMask = new Texture2D(size, size, TextureFormat.RGBAHalf, false, true);
         cacheMask.hideFlags = HideFlags.HideAndDontSave;
         cacheMask.filterMode = FilterMode.Point;
         cacheMask.wrapMode = TextureWrapMode.Clamp;
         cacheMask.SetPixels(cacheMaskBuffer);
         cacheMask.Apply(false, false);
      }

      void ClearCacheMaskBuffer()
      {
         // setup cache mask
         int size = (int)staticCacheSize;
         cacheMaskBuffer = new Color[size * size];

         Color black = new Color(0, 0, 0, 0);
         for (int x = 0; x < size; ++x)
         {
            for (int y = 0; y < size; ++y)
            {
               cacheMaskBuffer[y * size + x] = black;
            }
         }
      }

      void InitStatic()
      {
         maxStaticDecals = 1;
         if (msObj.keywordSO.IsKeywordEnabled("_DECAL_STATICMAX64"))
         {
            maxStaticDecals = 64;
         }
         else if (msObj.keywordSO.IsKeywordEnabled("_DECAL_STATICMAX128"))
         {
            maxStaticDecals = 128;
         }
         else if (msObj.keywordSO.IsKeywordEnabled("_DECAL_STATICMAX256"))
         {
            maxStaticDecals = 256;
         }
         else if (msObj.keywordSO.IsKeywordEnabled("_DECAL_STATICMAX512"))
         {
            maxStaticDecals = 512;
         }
         else if (msObj.keywordSO.IsKeywordEnabled("_DECAL_STATICMAX1024"))
         {
            maxStaticDecals = 1024;
         }
         else if (msObj.keywordSO.IsKeywordEnabled("_DECAL_STATICMAX2048"))
         {
            maxStaticDecals = 2048;
         }
         staticDecals = new List<MicroSplatDecal>(maxStaticDecals);
         if (Application.IsPlaying(this) && cacheMaskBuffer != null && cacheMaskBuffer.Length == maxStaticDecals * maxStaticDecals)
         {
            loadStaticFromCache = true;
         }
         else
         {
            ClearCacheMask();
            ClearStaticCacheData();
            needsStaticUpdate = true;
         }
      }

      void RegisterStaticDecal(MicroSplatDecal d)
      {
         staticDecals.Add(d);

         needsStaticUpdate = true;
      }

      void UnregisterStaticDecal(MicroSplatDecal d)
      {
         if (terrain != null)
         {
            // this might need to use the old data, in case we're out of sync..
            if (staticDecals != null && staticDecals.Contains(d))
            {
               staticDecals.Remove(d);
               needsStaticUpdate = true;
            }
         }
#if __MICROSPLAT_MESHTERRAIN__
         else if (meshTerrain != null)
         {
            if (staticDecals != null && staticDecals.Contains(d))
            {
               staticDecals.Remove(d);
               needsStaticUpdate = true;
            }
         }
#endif
      }

      void UpdateStaticCache()
      {
         if (staticDecals == null)
            return;
         int count = staticDecals.Count;
         if (count > maxStaticDecals)
            count = maxStaticDecals;

         for (int i = 0; i < count; ++i)
         {
            SetData(staticDecals[i], i, staticCacheData);
         }
         staticCacheData.Apply(false, false);
      }

      void UpdateStaticPropertyBlocks()
      {
         decalBlock.SetTexture("_DecalControl", cacheMask);
         decalBlock.SetTexture("_DecalStaticData", staticCacheData);
      }

      Vector2 WorldToTerrainPixel(Vector3 terrainPos, Vector3 terrainSize, Vector3 point, Texture2D splatControl)
      {
         point = point - terrainPos;
         float x = (point.x / terrainSize.x) * splatControl.width;
         float z = (point.z / terrainSize.z) * splatControl.height;
         return new Vector2(x, z);
      }

      Vector3 TerrainPixelToWorld(Vector3 terrainPos, Vector3 terrainSize, int x, int y, Texture2D splatControl)
      {
         Vector3 wp = new Vector3(x, 0, y);
         wp.x *= terrainSize.x / (float)splatControl.width;
         wp.z *= terrainSize.z / (float)splatControl.height;
         return wp += terrainPos;
      }

      Vector3 TerrainPixelToWorldWithHeight(Terrain t, Vector3 terrainPos, Vector3 terrainSize, int x, int y, Texture2D splatControl)
      {
         Vector3 wp = new Vector3(x, 0, y);
         wp.x *= terrainSize.x / (float)splatControl.width;
         wp.y = t.terrainData.GetInterpolatedHeight(x, y);
         wp.z *= terrainSize.z / (float)splatControl.height;
         return wp += terrainPos;
      }

      struct PixelBounds
      {
         public int xmin;
         public int xmax;
         public int ymin;
         public int ymax;
      }

      bool GetDecalPixelBounds(Vector3 terrainPos, Vector3 terrainSize, Matrix4x4 decalMtx, ref PixelBounds bounds)
      {
         // this seems to be texel size dependent.. Expect to fix bugs here..
         float sz = 0.5f;
         Bounds aabb = new Bounds(decalMtx.MultiplyPoint(new Vector3(-sz, -sz, -sz)), Vector3.one);
         aabb.Encapsulate(decalMtx.MultiplyPoint(new Vector3(sz, sz, sz)));
         aabb.Encapsulate(decalMtx.MultiplyPoint(new Vector3(-sz, sz, sz)));
         aabb.Encapsulate(decalMtx.MultiplyPoint(new Vector3(sz, -sz, sz)));
         aabb.Encapsulate(decalMtx.MultiplyPoint(new Vector3(sz, sz, -sz)));
         aabb.Encapsulate(decalMtx.MultiplyPoint(new Vector3(-sz, -sz, sz)));
         aabb.Encapsulate(decalMtx.MultiplyPoint(new Vector3(sz, -sz, -sz)));
         aabb.Encapsulate(decalMtx.MultiplyPoint(new Vector3(-sz, sz, -sz)));

         var min = (aabb.min);
         var max = (aabb.max);

         Vector2 minUV = WorldToTerrainPixel(terrainPos, terrainSize, min, cacheMask);
         Vector2 maxUV = WorldToTerrainPixel(terrainPos, terrainSize, max, cacheMask);

         // expand by an extra pixel. 
         bounds.xmin = Mathf.FloorToInt(minUV.x - 1);
         bounds.ymin = Mathf.FloorToInt(minUV.y - 1);
         bounds.xmax = Mathf.FloorToInt(maxUV.x + 1);
         bounds.ymax = Mathf.FloorToInt(maxUV.y + 1);


         // are we out of the terrain bounds? If so, skip
         if (bounds.xmin < 0 && bounds.xmax < 0)
            return false;
         if (bounds.ymin < 0 && bounds.ymax < 0)
            return false;
         if (bounds.xmin >= cacheMask.width && bounds.xmax >= cacheMask.width)
            return false;
         if (bounds.ymin >= cacheMask.height && bounds.ymax >= cacheMask.height)
            return false;

         // clamp to edges
         bounds.xmin = Mathf.Clamp(bounds.xmin, 0, cacheMask.width);
         bounds.xmax = Mathf.Clamp(bounds.xmax, 0, cacheMask.width);
         bounds.ymin = Mathf.Clamp(bounds.ymin, 0, cacheMask.height);
         bounds.ymax = Mathf.Clamp(bounds.ymax, 0, cacheMask.height);

         // make sure we are at least one pixel
         if (bounds.xmin == bounds.xmax)
         {
            if (bounds.xmax < cacheMask.width)
               bounds.xmax += 1;
            else
               bounds.xmin--;
         }
         if (bounds.ymin == bounds.ymax)
         {
            if (bounds.ymax < cacheMask.height)
               bounds.ymax += 1;
            else
               bounds.ymin--;
         }

         return true;
      }

      bool PointInOABB(Vector3 pt, Matrix4x4 decalMtx)
      {
         var point = decalMtx.MultiplyPoint(pt);
         if (point.x < 1 && point.x > -1 &&
            point.y < 1 && point.y > -1 &&
            point.z < 1 && point.z > -1)
            return true;
         else
            return false;
      }

      void ClearDecalInCache(Vector3 terrainPos, Vector3 terrainSize, Matrix4x4 dmtx, int index, PixelBounds pb)
      {
         int sz = cacheMask.width;
         for (int x = pb.xmin; x < pb.xmax; ++x)
         {
            for (int y = pb.ymin; y < pb.ymax; ++y)
            {
               /* TODO: Make this clip for extra speed
               var wp = TerrainPixelToWorld (terrainPos, terrainSize, x, y, cacheMask);
               var wp1 = TerrainPixelToWorld (terrainPos, terrainSize, x - 1, y - 1, cacheMask);
               var wp2 = TerrainPixelToWorld (terrainPos, terrainSize, x + 1, y + 1, cacheMask);
               if (PointInOABB (wp, dmtx) || PointInOABB (wp1, dmtx) || PointInOABB (wp2, dmtx))
               */
               {
                  int cidx = y * sz + x;
                  Color values = cacheMaskBuffer[cidx];

                  if (Mathf.RoundToInt(values.r - 1) == index)
                  {
                     values.r = values.g;
                     values.g = values.b;
                     values.b = values.a;
                     values.a = 0;
                     cacheMaskBuffer[cidx] = values;
                  }
                  else if (Mathf.RoundToInt(values.g - 1) == index)
                  {
                     values.g = values.b;
                     values.b = values.a;
                     values.a = 0;
                     cacheMaskBuffer[cidx] = values;
                  }
                  else if (Mathf.RoundToInt(values.b - 1) == index)
                  {
                     values.b = values.a;
                     values.a = 0;
                     cacheMaskBuffer[cidx] = values;
                  }
                  else if (Mathf.RoundToInt(values.a - 1) == index)
                  {
                     values.a = 0;
                     cacheMaskBuffer[cidx] = values;
                  }
               }
            }
         }
      }

      void ClearDecalInCache(Vector3 terrainPos, Vector3 terrainSize, MicroSplatDecal d, Matrix4x4 oldMtx, int index)
      {
         PixelBounds pb = new PixelBounds();
         if (GetDecalPixelBounds(terrainPos, terrainSize, oldMtx, ref pb))
         {
            ClearDecalInCache(terrainPos, terrainSize, oldMtx, index, pb);
         }
      }

      void RenderDecalIntoCache(int index, Vector3 terrainPos, Vector3 terrainSize, MicroSplatDecal d, PixelBounds pb)
      {
         // draw
         Transform tfm = d.transform;
         var dmtx = tfm.worldToLocalMatrix;

         int sz = cacheMask.width;

         for (int x = pb.xmin; x < pb.xmax; ++x)
         {
            for (int y = pb.ymin; y < pb.ymax; ++y)
            {
               // man, I wish I could get this to work. The idea is to clip the point if it's inside the scaled box. This shrinks the area to
               // be more exact, tightening up performance and preventing false decal overlaps. But for some reason, this only works on
               // a top down decal projection - once rotated, the box is clipped as if it's not rotated at all.

               //var wp = TerrainPixelToWorldWithHeight (t, terrainPos, terrainSize, x, y, cacheMask);
               //var wp1 = TerrainPixelToWorldWithHeight (t, terrainPos, terrainSize, x-1, y-1, cacheMask);
               //var wp2 = TerrainPixelToWorldWithHeight (t, terrainPos, terrainSize, x+1, y+1, cacheMask);
               //var wp3 = TerrainPixelToWorldWithHeight (t, terrainPos, terrainSize, x + 1, y - 1, cacheMask);
               //var wp4 = TerrainPixelToWorldWithHeight (t, terrainPos, terrainSize, x - 1, y + 1, cacheMask);
               //if (PointInOABB (wp, dmtx) || PointInOABB (wp1, dmtx) || PointInOABB (wp2, dmtx) || PointInOABB (wp3, dmtx) || PointInOABB (wp4, dmtx))
               {
                  int cidx = y * sz + x;
                  Color values = cacheMaskBuffer[cidx];

                  // skip if we're already in the cache
                  if (Mathf.RoundToInt(values.r - 1) != index &&
                     Mathf.RoundToInt(values.g - 1) != index &&
                     Mathf.RoundToInt(values.b - 1) != index &&
                     Mathf.RoundToInt(values.a - 1) != index)
                  {

                     if (values.r < 0.5f)
                     {
                        values.r = index + 1;
                        values.g = 0;
                        values.b = 0;
                        values.a = 0;
                     }
                     else if (values.g < 0.5f)
                     {
                        values.g = values.r;
                        values.r = index + 1;
                        values.b = 0;
                        values.a = 0;
                     }
                     else if (values.b < 0.5f)
                     {
                        values.b = values.g;
                        values.g = values.r;
                        values.r = index + 1;
                        values.a = 0;
                     }
                     else
                     {
                        values.a = values.b;
                        values.b = values.g;
                        values.g = values.r;
                        values.r = index + 1;
                     }
                     cacheMaskBuffer[cidx] = values;
                  }
               }
            }
         }
      }

      void RenderDecalIntoCache(Vector3 terrainPos, Vector3 terrainSize, MicroSplatDecal d, int index)
      {
         if (d.isActiveAndEnabled)
         {
            PixelBounds pb = new PixelBounds();
            if (GetDecalPixelBounds(terrainPos, terrainSize, d.transform.localToWorldMatrix, ref pb))
            {
               RenderDecalIntoCache(index, terrainPos, terrainSize, d, pb);
            }
         }
      }

      public void UpdateDecalInCache(Vector3 terrainPos, Vector3 terrainSize, MicroSplatDecal d, Matrix4x4 oldMtx)
      {
         UnityEngine.Profiling.Profiler.BeginSample("UpdateDecalInCache");
         int listIndex = staticDecals.IndexOf(d);
         ClearDecalInCache(terrainPos, terrainSize, d, oldMtx, listIndex);
         RenderDecalIntoCache(terrainPos, terrainSize, d, listIndex);
         cacheMask.SetPixels(cacheMaskBuffer);
         cacheMask.Apply(false, false);
         UpdateStaticPropertyBlocks();
         UpdateStaticCache();
         //UpdatePropertyBlocks ();
         UnityEngine.Profiling.Profiler.EndSample();
      }


      public void RerenderCacheMap()
      {
         UnityEngine.Profiling.Profiler.BeginSample("RerenderCacheMap");
         ClearCacheMaskBuffer();
         ClearCacheMask();
         SortStaticDecals();
         UpdateStaticCache();
         UpdatePropertyBlocks();
         if (terrain != null)
         {
            for (int i = 0; i < staticDecals.Count; ++i)
            {
               RenderDecalIntoCache(terrain.transform.position, terrain.terrainData.size, staticDecals[i], i);
            }
         }
         else
         {
#if __MICROSPLAT_MESHTERRAIN__
            if (meshTerrain != null)
            {
               for (int i = 0; i < staticDecals.Count; ++i)
               {
                  RenderDecalIntoCache(meshTerrain.transform.position, meshTerrain.GetBounds().size, staticDecals[i], i);
               }
            }
#endif
         }
         cacheMask.SetPixels(cacheMaskBuffer);
         cacheMask.Apply(false, false);
         staticCacheData.Apply(false, false);
         UnityEngine.Profiling.Profiler.EndSample();
      }

      // sort by hash, then order, that way sorting produces the same results every time
      void SortStaticDecals()
      {
         if (staticDecals == null)
            return;
         staticDecals.Sort((x, y) => x.GetHashCode().CompareTo(y.GetHashCode()));
         staticDecals.Sort((x, y) => x.sortOrder.CompareTo(y.sortOrder));
      }

      public void LoadFromCache()
      {
         ClearCacheMask();
         SortStaticDecals();
         UpdateStaticCache();
         UpdatePropertyBlocks();
         cacheMask.SetPixels(cacheMaskBuffer);
         cacheMask.Apply(false, false);
         staticCacheData.Apply(false, false);
      }
   }
}
