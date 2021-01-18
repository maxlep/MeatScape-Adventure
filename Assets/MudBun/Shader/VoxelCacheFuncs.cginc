/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_VOXEL_CACHE_FUNCS
#define MUDBUN_VOXEL_CACHE_FUNCS

#include "VoxelCacheDefs.cginc"

#include "AllocationDefs.cginc"
#include "BrushFuncs.cginc"
#include "Noise/RandomNoise.cginc"

#define kMaxCacheIterations (4)

uint cache_hash(float3 p)
{
  return uint(1e9f * rand(p));
}

// https://nosferalatu.com/SimpleGPUHashTable.html
float sdf_masked_brushes_cached(float3 p, int iBrushMask, out SdfBrushMaterial mat)
{
  float3 fiP = round(p / (0.5f * voxelSize));
  uint slot = cache_hash(fiP / voxelSize) % voxelCacheSize;
  
  uint i, s;
  
  // look up cache
  i = 0;
  s = slot;
  while (i++ < kMaxCacheIterations)
  {
    uint iCache = voxelCacheIdTable[s];
    if (iCache != kNullVoxelCacheId && all(voxelCache[iCache].data.xyz == fiP))
    {
      mat = voxelCache[iCache].material;
      //mat.color = pack_rgba(float4(1.0f, 0.0f, 0.0f, 1.0f));
      return voxelCache[iCache].data.w;
    }

    s = (s + 1) % voxelCacheSize;
  }

  // add cache
  int iCache;
  InterlockedAdd(aNumAllocation[kNumAllocationsVoxelCache], 1, iCache);
  float res = sdf_masked_brushes(p, iBrushMask, voxelCache[iCache].material);
  voxelCache[iCache].data = float4(fiP, res);
  i = 0;
  s = slot;
  while (i++ < kMaxCacheIterations)
  {
    uint prev = kNullVoxelCacheId;
    InterlockedCompareExchange(voxelCacheIdTable[s], kNullVoxelCacheId, iCache, prev);
    if (prev == kNullVoxelCacheId)
      break;

    // collision
    s = (s + 1) % voxelCacheSize;
  }
  
  mat = voxelCache[iCache].material;
  return res;
}

#endif

