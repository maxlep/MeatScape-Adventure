/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_CODEC
#define MUDBUN_CODEC

#include "MathConst.cginc"

// float2 betwen 0.0 and 1.0
// https://stackoverflow.com/questions/17638800/storing-two-float-values-in-a-single-float-variable
//-----------------------------------------------------------------------------

float pack_saturated(float2 v)
{
  const int kPrecision = 4096;
  v = floor(v * (kPrecision - 1));
  return v.x * kPrecision + v.y;
}

float2 unpack_saturated(float f)
{
  const int kPrecision = 4096;
  return float2(floor(f / kPrecision), f % kPrecision) / (kPrecision - 1);
}

//-----------------------------------------------------------------------------
// end: float2 between 0.0 and 1.0


// normals
// https://knarkowicz.wordpress.com/2014/04/16/octahedron-normal-vector-encoding/
//-----------------------------------------------------------------------------

float2 oct_wrap(float2 v)
{
  return (1.0f - abs(v.yx)) * (v.xy >= 0.0f ? 1.0f : -1.0f);
}

float pack_normal(float3 n)
{
  n /= max(kEpsilon, abs(n.x) + abs(n.y) + abs(n.z));
  n.xy = n.z >= 0.0 ? n.xy : oct_wrap(n.xy);
  n.xy = n.xy * 0.5 + 0.5;
  return pack_saturated(n.xy);
}

float3 unpack_normal(float f)
{
  float2 v = unpack_saturated(f);
  v = v * 2.0f - 1.0f;
  float3 n = float3(v.x, v.y, 1.0 - abs(v.x) - abs(v.y));
  float t = saturate(-n.z);
  n.xy += n.xy >= 0.0 ? -t : t;
  return normalize(n);
}

//-----------------------------------------------------------------------------
// end: normals


// colors
//-----------------------------------------------------------------------------

uint pack_rgb(float3 c) {
  return 
      (uint(c.z * 255) << 16) 
    | (uint(c.y * 255) <<  8) 
    | (uint(c.x * 255) <<  0);
}

float3 unpack_rgb(uint i)
{
  return
    float3
    (
      ((i & 0x000000FF) >>  0) / 255.0f, 
      ((i & 0x0000FF00) >>  8) / 255.0f, 
      ((i & 0x00FF0000) >> 16) / 255.0f
    );
}

uint pack_rgba(float4 c)
{
  return
      (uint(c.w * 255) << 24) 
    | (uint(c.z * 255) << 16) 
    | (uint(c.y * 255) <<  8) 
    | (uint(c.x * 255) <<  0);
}

float4 unpack_rgba(uint i)
{
  return
    float4
    (
      ((i & 0x000000FF) >>  0) / 255.0f, 
      ((i & 0x0000FF00) >>  8) / 255.0f, 
      ((i & 0x00FF0000) >> 16) / 255.0f, 
      ((i & 0xFF000000) >> 24) / 255.0f
    );
}

//-----------------------------------------------------------------------------
// end: colors


// hash
//-----------------------------------------------------------------------------

static const uint kFnvDefaultBasis = 2166136261u;
static const uint kFnvPrime = 16777619u;

uint fnv_hash_concat(uint hash, uint i)
{
  return (hash ^ i) * kFnvPrime;
}

uint fnv_hash_concat(uint hash, uint3 i)
{
  hash = fnv_hash_concat(hash, i.x);
  hash = fnv_hash_concat(hash, i.y);
  hash = fnv_hash_concat(hash, i.z);
  return hash;
}

//-----------------------------------------------------------------------------
// end: hash

#endif
