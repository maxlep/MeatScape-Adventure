/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_AABB_TREE_DEFS
#define MUDBUN_AABB_TREE_DEFS

#define kAabbTreeNodeStackSize (16) // max possible tree height allowed

struct Aabb
{
  float3 boundsMin;
  float3 boundsMax;
};

struct AabbNode
{
  Aabb aabb;
  int iParent;
  int iChildA;
  int iChildB;
  int iData;
};

StructuredBuffer<AabbNode> aabbTree;
int aabbRoot;

#endif

