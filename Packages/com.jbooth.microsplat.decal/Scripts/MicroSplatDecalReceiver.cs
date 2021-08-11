//////////////////////////////////////////////////////
// MicroSplat
// Copyright (c) Jason Booth
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JBooth.MicroSplat
{
   [ExecuteAlways]
   public partial class MicroSplatDecalReceiver : MonoBehaviour
   {
      /// <summary>
      /// The decal system has two pathways, the dynamic and static pathways.
      ///
      /// The dynamic pathay draws every decal
      /// for each pixel. This is not ideal because even with good culling, you end up with substancial overhead per pixel.
      /// Also, some platforms can only have a limited loop count, so the shader might not compile with a high dynamic decal
      /// limit.
      ///
      /// The static pathway is done with what I'm calling "Deferred Indexed Decals". When a static decal is moved, added, or removed
      /// it is software rendered into a small buffer. This buffer stores a list of up to 4 decals touching any area covered by the pixel.
      /// Thus, the shader can sample this "index map" and determine which 4 decals need to be sampled for that that area in the shader.
      /// This way you can have thousands of decals, but only pay the cost of up to 4 decals on any given pixel.
      ///
      /// Rendering this software buffer is realitively quick, but often too costly for runtime use. This is why the system is considered
      /// "static". In fact, it's fully dynamic, but just not quick enough to want to use in cases where you'd want decals moving around. 
      /// </summary>

      // Data Layout
      /// X = decal index
      /// Matrix X      0
      /// Matrux Y      1
      /// Matrix Z      2
      /// Matrix W      3
      /// Data1         4
      /// Data2         5
      /// tint          6
      /// splat indexes 7
      /// </summary>

      MaterialPropertyBlock decalBlock;
      public MicroSplatObject msObj { get; private set; }
      Terrain terrain;
      Renderer rend;
#if __MICROSPLAT_MESHTERRAIN__
      MicroSplatMeshTerrain meshTerrain;
#endif
      public bool generateCacheOnLoad = false;

      bool needsStaticUpdate = true;
      bool loadStaticFromCache = false;

      private void InitSystem()
      {
         if (decalBlock == null)
         {
            decalBlock = new MaterialPropertyBlock();
            msObj = GetComponent<MicroSplatObject>();
            if (msObj == null)
            {
               Debug.LogError("MicroSplatDecalReceiver must be on MicroSplat Object");
            }
            else
            {
               terrain = GetComponent<Terrain>();
               rend = GetComponent<Renderer>();
#if __MICROSPLAT_MESHTERRAIN__
               meshTerrain = GetComponent<MicroSplatMeshTerrain>();
#endif
               if (msObj.keywordSO == null)
               {
                  Debug.LogError("MicroSplatDecalReceiver cannot find keyword data on MicroSplatObject, please make sure this is assigned");
               }
            }

            InitStatic();
            InitDynamic();
         }
      }

      public bool RegisterDecal(MicroSplatDecal d)
      {
         if (decalBlock == null)
         {
            InitSystem();
         }
         if (terrain != null)
         {
            if (d.dynamic)
            {
               RegisterDynamicDecal(d);
               return true;
            }
            else
            {
               RegisterStaticDecal(d);
               return false;
            }
         }
#if __MICROSPLAT_MESHTERRAIN__
         else if (meshTerrain != null)
         {
            if (d.dynamic)
            {
               RegisterDynamicDecal(d);
               return true;
            }
            else
            {
               RegisterStaticDecal(d);
               return false;
            }
         }
#endif
         else
         {
            RegisterDynamicDecal(d);
            return true;
         }
      }

      public void UnregisterDecal(MicroSplatDecal d)
      {
         if (terrain != null)
         {
            if (d.dynamic)
            {
               UnregisterDynamicDecal(d);
            }
            else
            {
               UnregisterStaticDecal(d);
            }
         }
#if __MICROSPLAT_MESHTERRAIN__
         else if (meshTerrain != null)
         {
            if (d.dynamic)
            {
               UnregisterDynamicDecal(d);
            }
            else
            {
               UnregisterDynamicDecal(d);
            }
         }
#endif
         else
         {
            UnregisterDynamicDecal(d);
         }
      }

      void SetData(MicroSplatDecal d, int index, Texture2D tex)
      {
         if (d == null)
            return;
         var mtx = d.transform.worldToLocalMatrix;
         tex.SetPixel(index, 0, new Color(mtx.m00, mtx.m01, mtx.m02, mtx.m03));
         tex.SetPixel(index, 1, new Color(mtx.m10, mtx.m11, mtx.m12, mtx.m13));
         tex.SetPixel(index, 2, new Color(mtx.m20, mtx.m21, mtx.m22, mtx.m23));
         tex.SetPixel(index, 3, new Color(mtx.m30, mtx.m31, mtx.m32, mtx.m33));
         Vector4 data1, data2;
         d.GetShaderData(out data1, out data2);
         tex.SetPixel(index, 4, data1);
         tex.SetPixel(index, 5, data2);
         tex.SetPixel(index, 6, d.splatIndexes);
         tex.SetPixel(index, 7, d.tint);
      }

      private void OnEnable()
      {
         InitSystem();
      }

      private void OnDisable()
      {
         decalBlock = null;
      }

      private void OnDestroy()
      {
         decalBlock = null;
         if (staticCacheData)
         {
            DestroyImmediate(staticCacheData);
         }
         if (cacheMask != null)
         {
            DestroyImmediate(cacheMask);
         }
         if (dynamicCacheData)
         {
            DestroyImmediate(dynamicCacheData);
         }
         if (dynamicCullData)
         {
            DestroyImmediate(dynamicCullData);
         }
      }

      private void Update()
      {
         UpdatePropertyBlocks();
         if (needsStaticUpdate)
         {
            needsStaticUpdate = false;
            UpdateStaticCache();


            if (generateCacheOnLoad == false && loadStaticFromCache)
            {
               loadStaticFromCache = false;
               LoadFromCache();
            }
            else
            {
               RerenderCacheMap();
            }
         }
      }

      void UpdatePropertyBlocks()
      {
         if (decalBlock == null)
            return;
         if (terrain != null)
         {
            terrain.GetSplatMaterialPropertyBlock(decalBlock);
         }
#if __MICROSPLAT_MESHTERRAIN__
         else if (meshTerrain != null)
         {
            if (meshTerrain.meshTerrains.Length > 0 && meshTerrain.meshTerrains[0] != null)
            {
               meshTerrain.meshTerrains[0].GetPropertyBlock(decalBlock);
            }
         }
#endif
         else if (rend != null)
         {
            rend.GetPropertyBlock(decalBlock);
         }

         UpdateDynamicPropertyBlocks();
         UpdateStaticPropertyBlocks();

         if (terrain != null)
         {
            terrain.SetSplatMaterialPropertyBlock(decalBlock);
         }
#if __MICROSPLAT_MESHTERRAIN__
         else if (meshTerrain != null)
         {
            for (int i = 0; i < meshTerrain.meshTerrains.Length; ++i)
            {
               var r = meshTerrain.meshTerrains[i];
               if (r != null)
               {
                  r.SetPropertyBlock(decalBlock);
               }
            }
         }
#endif
         else if (rend != null)
         {
            rend.SetPropertyBlock(decalBlock);
         }

      }

   }
}