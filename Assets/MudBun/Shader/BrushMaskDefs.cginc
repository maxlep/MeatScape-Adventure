/*****************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_BRUSH_MASK_DEFS
#define MUDBUN_BRUSH_MASK_DEFS

#define kBitsPerInt (32u)
#define kMaxBrushMaskInts (32u)
#define BRUSH_MASK(mask) uint mask[kMaxBrushMaskInts]

RWStructuredBuffer<uint> brushMaskPool;
int brushMaskPoolSize;

#endif

