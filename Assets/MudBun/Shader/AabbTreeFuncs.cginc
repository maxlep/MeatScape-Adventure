/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_AABB_TREE_FUNCS
#define MUDBUN_AABB_TREE_FUNCS

#include "AabbTreeDefs.cginc"

#include "Math/MathConst.cginc"

Aabb make_aabb(float3 boundsMin, float3 boundsMax)
{
  Aabb aabb;
  aabb.boundsMin = boundsMin;
  aabb.boundsMax = boundsMax;
  return aabb;
}

float3 aabb_center(Aabb aabb)
{
  return 0.5f * (aabb.boundsMin + aabb.boundsMax);
}

float3 aabb_size(Aabb aabb)
{
  return aabb.boundsMax - aabb.boundsMin;
}

float3 aabb_extents(Aabb aabb)
{
  return 0.5f * (aabb.boundsMax - aabb.boundsMin);
}

 bool aabb_intersects(Aabb a, Aabb b)
{
  return all(a.boundsMin <= b.boundsMax && a.boundsMax >= b.boundsMin);
}

float aabb_ray_cast(Aabb aabb, float3 from, float3 to)
{
  float tMin = -kFltMax;
  float tMax = +kFltMax;

  float3 d = to - from;
  float3 absD = abs(d);

  if (any(absD < kEpsilon))
  {
    // parallel?
    if (any(from < aabb.boundsMin) || any(aabb.boundsMax < from))
      return -kFltMax;
  }
  else
  {
    float3 invD = 1.0f / max(kEpsilon, d);
    float3 t1 = (aabb.boundsMin - from) * invD;
    float3 t2 = (aabb.boundsMax - from) * invD;
    float3 minComps = min(t1, t2);
    float3 maxComps = max(t1, t2);

    tMin = max(minComps.x, max(minComps.y, minComps.z));
    tMax = min(maxComps.x, min(maxComps.y, maxComps.z));
  }

  if (tMin > tMax)
    return -kFltMax;

  if (tMin < 0.0f)
    return -kFltMax;

  return tMin;
}

// stmt = statements processing "iData" of intersected leaf AABB nodes
// will gracefully handle maxed-out stacks
#define AABB_TREE_QUERY(tree, root, queryAabb, stmt)                          \
{                                                                             \
  bool touchedAnyBounds = false;                                              \
                                                                              \
  int stackTop = 0;                                                           \
  int stack[kAabbTreeNodeStackSize];                                          \
  stack[stackTop] = root;                                                     \
                                                                              \
  while (stackTop >= 0)                                                       \
  {                                                                           \
    int index = stack[stackTop--];                                            \
    if (index < 0)                                                            \
      continue;                                                               \
                                                                              \
    if (!aabb_intersects(tree[index].aabb, queryAabb))                        \
      continue;                                                               \
                                                                              \
    if (tree[index].iChildA < 0)                                              \
    {                                                                         \
      const int iData = tree[index].iData;                                    \
                                                                              \
      stmt                                                                    \
    }                                                                         \
    else                                                                      \
    {                                                                         \
      stackTop = min(stackTop + 1, kAabbTreeNodeStackSize - 1);               \
      stack[stackTop] = tree[index].iChildA;                                  \
      stackTop = min(stackTop + 1, kAabbTreeNodeStackSize - 1);               \
      stack[stackTop] = tree[index].iChildB;                                  \
    }                                                                         \
  }                                                                           \
}

// stmt = statements processing "iData" of hit leaf AABB nodes
// will gracefully handle maxed-out stacks
#define AABB_TREE_RAY_CAST(tree, root, rayFrom, rayTo, stmt)                  \
{                                                                             \
  float3 rayDir = normalize(rayTo - rayFrom);                                 \
  float3 rayDirOrtho = normalize(find_ortho(rayDir));                         \
  float3 rayDirOrthoAbs = abs(rayDirOrtho);                                   \
                                                                              \
  Aabb rayBounds;                                                             \
  rayBounds.boundsMin = float4(min(rayFrom, rayTo), 0.0f);                    \
  rayBounds.boundsMax = float4(max(rayFrom, rayTo), 0.0f);                    \
                                                                              \
  int stackTop = 0;                                                           \
  int stack[kAabbTreeNodeStackSize];                                          \
  stack[stackTop] = root;                                                     \
                                                                              \
  while (stackTop >= 0)                                                       \
  {                                                                           \
    int index = stack[stackTop--];                                            \
    if (index < 0)                                                            \
      continue;                                                               \
                                                                              \
    if (!aabb_intersects(tree[index].aabb, rayBounds))                        \
      continue;                                                               \
                                                                              \
    float3 aabbCenter = aabb_center(tree[index].aabb);                        \
    float3 aabbHalfExtents = aabb_extents(tree[index].aabb);                  \
    float separation =                                                        \
      abs(dot(rayDirOrtho, rayFrom - aabbCenter))                             \
      - dot(rayDirOrthoAbs, aabbHalfExtents);                                 \
    if (separation > 0.0f)                                                    \
      continue;                                                               \
                                                                              \
    if (tree[index].iChildA < 0)                                              \
    {                                                                         \
      float t = aabb_ray_cast(tree[index].aabb, rayFrom, rayTo);              \
      if (t < 0.0f)                                                           \
        continue;                                                             \
                                                                              \
      const int iData = tree[index].iData;                                    \
                                                                              \
      stmt                                                                    \
    }                                                                         \
    else                                                                      \
    {                                                                         \
      stackTop = min(stackTop + 1, kAabbTreeNodeStackSize - 1);               \
      stack[stackTop] = tree[index].iChildA;                                  \
      stackTop = min(stackTop + 1, kAabbTreeNodeStackSize - 1);               \
      stack[stackTop] = tree[index].iChildB;                                  \
    }                                                                         \
  }                                                                           \
}

#endif

