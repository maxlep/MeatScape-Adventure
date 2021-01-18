/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_VOXEL_CACHE_DEFS
#define MUDBUN_VOXEL_CACHE_DEFS

#include "BrushDefs.cginc"

#define kNullVoxelCacheId (1u << 31)
#define kVoxelCacheMiss   (0.9f * kFltMax)

struct VoxelCacheData
{
  float4 data;
  SdfBrushMaterial material;
};

bool useVoxelCache;
RWStructuredBuffer<uint> voxelCacheIdTable;
RWStructuredBuffer<VoxelCacheData> voxelCache;
int voxelCacheSize;

#endif

