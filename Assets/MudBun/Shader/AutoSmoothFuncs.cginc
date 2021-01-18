/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_AUTO_SMOOTH_FUNCS
#define MUDBUN_AUTO_SMOOTH_FUNCS

#include "AutoSmoothDefs.cginc"

#include "AllocationDefs.cginc"
#include "Math/Codec.cginc"
#include "Math/MathConst.cginc"
#include "Math/Vector.cginc"
#include "Noise/RandomNoise.cginc"
#include "VoxelDefs.cginc"

uint auto_smooth_vert_data_id(float3 p)
{
  int3 q = int3(round(p / (0.125f * voxelSize)));
  uint hash = kFnvDefaultBasis;
  hash = fnv_hash_concat(hash, uint(q.x + 0x80000000));
  hash = fnv_hash_concat(hash, uint(q.y + 0x80000000));
  hash = fnv_hash_concat(hash, uint(q.z + 0x80000000));
  return (hash << 1) | 1;
}

int update_auto_smooth_vert_data(uint id, float packedNormal, float weight)
{
  uint slot = id % autoSmoothVertDataPoolSize;
  weight = max(kEpsilon, weight);

  int i = 0;
  while (i++ < autoSmoothVertDataPoolSize)
  {
    uint prev = kNullAutoSmoothCacheId;
    InterlockedCompareExchange(autoSmoothVertDataTable[slot].id, kNullAutoSmoothCacheId, id, prev);

    if (prev == kNullAutoSmoothCacheId             // newly registered
        || id == autoSmoothVertDataTable[slot].id) // already registered
    {
      uint iNormal = 0xFF;
      InterlockedAdd(autoSmoothVertDataTable[slot].numNormals, 1, iNormal);
      if (iNormal < kMaxAutoSmoothNormalPerVertex)
      {
        autoSmoothVertDataTable[slot].aNormal[iNormal] = packedNormal;
        autoSmoothVertDataTable[slot].aWeight[iNormal] = weight;
      }
      else
      {
        return -1;
      }

      return slot;
    }
    else
    {
      // ID collision
      slot = (slot + 1) % autoSmoothVertDataPoolSize;
    }
  }
  return -1;
}

float3 compute_auto_smooth_normal(uint id, float3 vertNormal)
{
  uint slot = id % autoSmoothVertDataPoolSize;

  // look up data
  int i = 0;
  while (i++ < autoSmoothVertDataPoolSize)
  {
    if (autoSmoothVertDataTable[slot].id == kNullAutoSmoothCacheId)
      return float3(0.0f, 0.0f, 0.0f); // total ID miss

    if (autoSmoothVertDataTable[slot].id == id)
    {
      // ID hit
      float3 totalAutoSmoothNormal = 0.0f;
      for (uint iNormal = 0; iNormal < autoSmoothVertDataTable[slot].numNormals; ++iNormal)
      {
        float3 n = unpack_normal(autoSmoothVertDataTable[slot].aNormal[iNormal]);
        float weight = autoSmoothVertDataTable[slot].aWeight[iNormal];
        float3 weightedN = n * weight;
        if (acos(clamp(dot(n, vertNormal), -1.0f, 1.0f)) < autoSmoothMaxAngle + kEpsilon)
          totalAutoSmoothNormal += weightedN;
      }
      //return autoSmoothVertDataTable[slot].numNormals > kMaxAutoSmoothNormalPerVertex ? float3(1.0f, 0.0f, 0.0f) : float3(0.0f, 1.0f, 0.0f);
      return normalize_safe(totalAutoSmoothNormal, vertNormal);
    }

    // ID miss
    slot = (slot + 1) % autoSmoothVertDataPoolSize;
  }
  return float3(0.0f, 0.0f, 0.0f);
}

#endif

