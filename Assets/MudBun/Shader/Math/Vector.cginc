/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_VECTOR
#define MUDBUN_VECTOR

#include "MathConst.cginc"

#define kUnitX  (float3(1.0f, 0.0f, 0.0f))
#define kUnitY  (float3(0.0f, 1.0f, 0.0f))
#define kUnitZ  (float3(0.0f, 0.0f, 1.0f))
#define kOrigin (float3(0.0f, 0.0f, 0.0f))

float3 normalize_safe(float3 v, float3 fallback, float epsilon)
{
  float vv = dot(v, v);
  return vv > epsilon ? v / sqrt(vv) : fallback;
}

float3 normalize_safe(float3 v, float3 fallback)
{
  return normalize_safe(v, fallback, kEpsilon);
}

float3 normalize_safe(float3 v)
{
  return normalize_safe(v, kUnitZ);
}

float3 project_vec(float3 v, float3 onto)
{
  onto = normalize(onto);
  return dot(v, onto) * onto;
}

float3 project_plane(float3 v, float3 n)
{
  return v - project_vec(v, n);
}

float3 limit_length(float3 v, float maxLen)
{
  return min(maxLen, length(v)) * normalize_safe(v, 0.0f);
}

float3 find_ortho(float3 v)
{
  if (v.x >= kSqrt3Inv)
    return float3(v.y, -v.x, 0.0);
  else
    return float3(0.0, v.z, -v.y);
}

float3 find_ortho_consistent(float3 v)
{
  return normalize_safe(cross(v, kUnitY), kUnitX);
}

void form_orthonormal_basis(float3 v, out float3 a, out float3 b)
{
  a = normalize(find_ortho(v));
  b = cross(v, a);
}

// a and b must be normalized
float angle_between(float3 a, float3 b)
{
  return acos(clamp(dot(a, b), -1.0f, 1.0f));
}

float3 slerp(float3 a, float3 b, float t)
{
  float d = dot(normalize(a), normalize(b));
  if (d > kEpsilonComp)
  {
    return lerp(a, b, t);
  }

  float r = acos(clamp(d, -1.0f, 1.0f));
  return (sin((1.0 - t) * r) * a + sin(t * r) * b) / sin(r);
}

float3 nlerp(float3 a, float b, float t)
{
  return normalize(lerp(a, b, t));
}

float3x3 mat_basis(float3 xAxis, float3 yAxis, float3 zAxis)
{
  return transpose(float3x3(xAxis, yAxis, zAxis));
}

float3x3 mat_look_at(float3 dir, float3 up)
{
  float3 zAxis = normalize_safe(dir, kUnitZ);
  float3 xAxis = normalize_safe(cross(up, zAxis), kUnitX);
  float3 yAxis = cross(zAxis, xAxis);
  return mat_basis(xAxis, yAxis, zAxis);
}

float3 cartesian_to_spherical(float3 p)
{
  float r = length(p);
  return float3(r, atan2(p.z, p.x), acos(p.y / r));
}

float3 spherical_to_cartesian(float3 p)
{
  float s = sin(p.z);
  return p.x * float3(cos(p.y) * s, cos(p.z), sin(p.y) * s);
}

float comp_sum(float2 v)
{
  return dot(v, float2(1.0f, 1.0f));
}

float comp_sum(float3 v)
{
  return dot(v, float3(1.0f, 1.0f, 1.0f));
}

float comp_sum(float4 v)
{
  return dot(v, float4(1.0f, 1.0f, 1.0f, 1.0f));
}

float min_comp(float3 v)
{
  return min(v.x, min(v.y, v.z));
}

float max_comp(float3 v)
{
  return max(v.x, max(v.y, v.z));
}

float3 quantize(float3 v, float step)
{
  return round(v / step) * step;
}

#endif
