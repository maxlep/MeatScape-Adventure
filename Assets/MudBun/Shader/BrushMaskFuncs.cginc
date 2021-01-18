/*****************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_BRUSH_MASKS_FUNCS
#define MUDBUN_BRUSH_MASKS_FUNCS

#include "BrushMaskDefs.cginc"

#include "AllocationDefs.cginc"
#include "NormalFuncs.cginc"
#include "VoxelDefs.cginc"

int num_brush_mask_ints()
{
  return uint(numBrushes + kBitsPerInt - 1) / kBitsPerInt;
}

#define BRUSH_MASK_SET_ALL(mask)                                               \
{                                                                              \
  for (int iInt = 0, n = num_brush_mask_ints(); iInt < n; ++iInt)              \
    mask[iInt] = (~0u);                                                        \
}

#define BRUSH_MASK_CLEAR_ALL(mask)                                             \
{                                                                              \
  for (int iInt = 0, n = num_brush_mask_ints(); iInt < n; ++iInt)              \
    mask[iInt] = 0u;                                                           \
}

#define BRUSH_MASK_SET(mask, bit)                                              \
  (mask[uint(bit) / kBitsPerInt] |= (1u << (uint(bit) % kBitsPerInt)))

#define BRUSH_MASK_CLEAR(mask, bit)                                            \
  (mask[uint(bit) / kBitsPerInt] &= (~(1u << (uint(bit) % kBitsPerInt))))

#define BRUSH_MASK_GET(mask, bit)                                              \
  ((mask[uint(bit) / kBitsPerInt] & (1u << (uint(bit) % kBitsPerInt))) != 0u)

#define READ_BRUSH_MASK(mask, iMask)                                           \
{                                                                              \
  int numInts = num_brush_mask_ints();                                         \
  int iBaseInt = iMask * numInts;                                              \
  for (int iInt = 0; iInt < numInts; ++iInt)                                   \
    mask[iInt] = brushMaskPool[iBaseInt + iInt];                               \
}

#define WRITE_BRUSH_MASK(mask, iMask)                                          \
{                                                                              \
  int numInts = num_brush_mask_ints();                                         \
  int iBaseInt = iMask * numInts;                                              \
  for (int iInt = 0; iInt < numInts; ++iInt)                                   \
    brushMaskPool[iBaseInt + iInt] = mask[iInt];                               \
}

// https://www.geeksforgeeks.org/position-of-rightmost-set-bit/
#define FOR_EACH_BRUSH(iMask, stmt)                                            \
{                                                                              \
  int numInts = num_brush_mask_ints();                                         \
  int iBaseInt = iMask * numInts;                                              \
  int iBrushBase = 0;                                                          \
  for (int iInt = 0; iInt < numInts; ++iInt, iBrushBase += kBitsPerInt)        \
  {                                                                            \
    uint currInt = brushMaskPool[iBaseInt + iInt];                             \
    while (currInt != 0)                                                       \
    {                                                                          \
      int iFirstSetBit = int(log2(currInt & (~currInt + 1u)));                 \
      int iBrush = iBrushBase + iFirstSetBit;                                  \
      if (iBrush >= numBrushes)                                                \
        break;                                                                 \
      currInt &= ~(1u << iFirstSetBit);                                        \
                                                                               \
      if (aBrush[iBrush].type == kSdfNoOp)                                     \
        continue;                                                              \
                                                                               \
      if ((aBrush[iBrush].flags & kSdfBrushFlagsHidden) != 0)                  \
        continue;                                                              \
                                                                               \
      stmt                                                                     \
    }                                                                          \
  }                                                                            \
}

int allocate_brush_mask()
{
  int iMask = -1;
  InterlockedAdd(aNumAllocation[kNumAllocationsBrushMasks], 1, iMask);

  if (iMask >= brushMaskPoolSize)
  {
    aNumAllocation[kNumAllocationsBrushMasks] = brushMaskPoolSize;
    iMask = -1;
  }

  return iMask;
}

int allocate_node_brush_mask(int iNode, Aabb nodeAabb)
{
  int iBrushMask = allocate_brush_mask();
  if (iBrushMask < 0)
    return -1;

  float3 clampedSurfaceShift = max(-aabb_extents(nodeAabb), surfaceShift);
  nodeAabb.boundsMin.xyz -= clampedSurfaceShift;
  nodeAabb.boundsMax.xyz += clampedSurfaceShift;

  BRUSH_MASK(brushMask);

  if (forceAllBrushes)
  {
    BRUSH_MASK_SET_ALL(brushMask);
  }
  else
  {
    BRUSH_MASK_CLEAR_ALL(brushMask);

    int iParent = nodePool[iNode].iParent;
    int iParentMask = iParent >= 0 ? nodePool[iParent].iBrushMask : -1;
    if (iParentMask >= 0)
    {
      float maxBlend = 0.0f;

      FOR_EACH_BRUSH(iParentMask, 
        if (aabb_intersects(nodeAabb, aabbTree[aBrush[iBrush].iProxy].aabb))
          maxBlend = max(maxBlend, aBrush[iBrush].blend);
      );

      nodeAabb.boundsMin -= maxBlend + voxelSize + normalDifferentiationStep;
      nodeAabb.boundsMax += maxBlend + voxelSize + normalDifferentiationStep;

      FOR_EACH_BRUSH(iParentMask, 
        if (aabb_intersects(nodeAabb, aabbTree[aBrush[iBrush].iProxy].aabb))
          BRUSH_MASK_SET(brushMask, iBrush);
      );
    }
    else
    {
      float maxBlend = 0.0f;

      AABB_TREE_QUERY(aabbTree, aabbRoot, nodeAabb, 
        maxBlend = max(maxBlend, aBrush[iData].blend);
      );

      nodeAabb.boundsMin -= maxBlend + normalDifferentiationStep;
      nodeAabb.boundsMax += maxBlend + normalDifferentiationStep;

      AABB_TREE_QUERY(aabbTree, aabbRoot, nodeAabb, 
        BRUSH_MASK_SET(brushMask, iData);
      );
    }
  }

  WRITE_BRUSH_MASK(brushMask, iBrushMask);

  return iBrushMask;
}

int get_brush_mask_index(int iNode)
{
  int iBrushMask = nodePool[iNode].iBrushMask;
  while (iBrushMask < 0)
  {
    iNode = nodePool[iNode].iParent;
    if (iNode < 0)
      break;

    iBrushMask = nodePool[iNode].iBrushMask;
  }

  return iBrushMask;
}

#endif

