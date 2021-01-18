/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_VOXEL_FUNCS
#define MUDBUN_VOXEL_FUNCS

#include "VoxelDefs.cginc"

int allocate_node(float3 center, int nodeDepth, int iParent)
{
  int iNode;
  InterlockedAdd(aNumNodesAllocated[nodeDepth + 1], 1, iNode);
  InterlockedAdd(aNumNodesAllocated[0], 1, iNode);

  if (iNode < int(nodePoolSize))
  {
    nodePool[iNode].center = center;
    nodePool[iNode].iParent = iParent;
    nodePool[iNode].iBrushMask = -1;
  }
  else
  {
    aNumNodesAllocated[0] = nodePoolSize;
    iNode = -1;
  }

  return iNode;
}

#endif

