/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_VOXEL_HASH_DEFS
#define MUDBUN_VOXEL_HASH_DEFS

#define kNullVoxelHashId (0)

struct VoxelHashEntry
{
  uint id;
};

RWStructuredBuffer<VoxelHashEntry> nodeHashTable;
int nodeHashTableSize;

#endif

