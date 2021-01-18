/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_SDF_PRIMITIVES
#define MUDBUN_SDF_PRIMITIVES

#include "../Math/Vector.cginc"
#include "../Noise/CachedNoise3D.cginc"
#include "../Noise/ClassicNoise3D.cginc"
#include "../Noise/TriangleNoise3D.cginc"

// https://iquilezles.org/www/articles/distfunctions/distfunctions.htm

// r: radius
float sdf_sphere(float3 p, float r)
{
  return length(p) - r;
}

// h: half extents
float sdf_ellipsoid(float3 p, float3 h)
{
  float k0 = max(kEpsilon, length(p / h));
  float k1 = max(kEpsilon, length(p / (h * h)));
  return k0 * (k0 - 1.0f) / k1;
}

// c: center
// h: half extents
// r: round
float sdf_box(float3 p, float3 h, float r = 0.0f)
{
  h = abs(h);
  float3 d = abs(p) - h;
  return length(max(d, 0.0f)) + min(max_comp(d), 0.0f) - r;
}

// a: point A
// b: point B
// r: radius
float sdf_capsule(float3 p, float3 a, float3 b, float r)
{
  float3 ab = b - a;
  float3 ap = p - a;
  p -= a + saturate(dot(ap, ab) / dot(ab, ab)) * ab;
  return length(p) - r;
}

// h:  height
// r1: radius 1
// r2: radius 2
// r:  round
float sdf_capped_cone(float3 p, float h, float r1, float r2, float r = 0.0f)
{
  float2 q = float2(length(p.xz), p.y);
  float2 k1 = float2(r2, h);
  float2 k2 = float2(r2 - r1, 2.0f * h);
  float2 ca = float2(q.x - min(q.x, (q.y < 0.0f) ? r1 : r2), abs(q.y) - h);
  float2 cb = q - k1 + k2 * clamp(dot(k1 - q, k2) / dot(k2, k2), 0.0f, 1.0f);
  float s = (cb.x < 0.0f && ca.y < 0.0f) ? -1.0f : 1.0f;
  return s * sqrt(min(dot(ca, ca), dot(cb, cb))) - r;
}

// h:  height
// r:  radius
// rr: extra radius
float sdf_cylinder(float3 p, float h, float r, float rr = 0.0f)
{
  float2 d = abs(float2(length(p.xz), p.y)) - float2(r, h);
  return min(max(d.x, d.y), 0.0f) + length(max(d, 0.0f)) - rr;
}

float sdf_torus(float3 p, float h, float r1, float r2)
{
  float3 q = float3(max(abs(p.x) - h, 0.0f), p.y, p.z);
  return length(float2(length(q.xz) - r1, q.y)) - r2;
}

float sdf_solid_angle(float3 p, float2 c, float r, float rr = 0.0f)
{
  // c is the sin/cos of the angle
  float2 q = float2(length(p.xz), p.y);
  float l = length(q) - r;
  float m = length(q - c * clamp(dot(q, c), 0.0f, r));
  return max(l, m*sign(c.y * q.x - c.x * q.y)) - rr;
}

float sdf_noise(int type, float3 p, float3 boundsMin, float3 boundsMax, float3 offset, float3 size, float threshold, int numOctaves, float octaveOffsetFactor, float3 period = 100.0f)
{
  float n = 0.0f;
  float f = 1.0f;
  switch (type)
  {
    case kSdfNoiseTypeCachedPerlin:
      //n = threshold - (0.8f * (saturate(pnoise(p / size, offset, numOctaves, octaveOffsetFactor, period)) - 0.5f) + 0.5f);
      n = cached_noise(p / size, offset, numOctaves, octaveOffsetFactor);
      f = 1.0f;
      break;

    case kSdfNoiseTypeTriangle:
      n = triangle_noise(p / size, offset, numOctaves, octaveOffsetFactor);
      f = 0.4f;
      break;
  }

  float d = threshold - n;

  // noise is not an actual SDF
  // we need to scale the result to make it behave like one
  // making the result slightly smaller than it should be would prevent false positive voxel node culling
  d *= f * min(min(size.x, size.y), size.z);

  return d;
}

float sdf_round_cone(float3 p, float3 a, float3 b, float r1, float r2)
{
  // sampling independent computations (only depend on shape)
  float3 ba = b - a;
  float l2 = dot(ba, ba);
  float rr = r1 - r2;
  float a2 = l2 - rr * rr;
  float il2 = 1.0f / l2;

  // sampling dependant computations
  float3 pa = p - a;
  float y = dot(pa, ba);
  float z = y - l2;
  float3 g = pa * l2 - ba * y;
  float x2 = dot(g, g);
  float y2 = y * y * l2;
  float z2 = z * z * l2;

  // single square root!
  float k = sign(rr) * rr * rr * x2;
  if (sign(z) * a2 * z2 > k) 
    return sqrt(x2 + z2) * il2 - r2;

  if (sign(y) * a2 * y2 < k) 
    return sqrt(x2 + y2) * il2 - r1;

  return (sqrt(x2*a2*il2) + y * rr)*il2 - r1;
}

// https://www.shadertoy.com/view/MsXGWr
float2 sdf_segment(float3 p, float3 a, float3 b)
{
  float3 pa = p - a, ba = b - a;
  float h = saturate(dot(pa, ba) / dot(ba, ba));
  return float2(length(pa - ba * h), h);
}

// https://www.shadertoy.com/view/ldj3Wh
float2 sdf_bezier(float3 pos, float3 A, float3 B, float3 C)
{
  float3 a = B - A;
  float3 b = A - 2.0f * B + C;
  float3 c = a * 2.0f;
  float3 d = A - pos;

  float kk = 1.0f / dot(b, b);
  float kx = kk * dot(a, b);
  float ky = kk * (2.0f * dot(a, a) + dot(d, b)) / 3.0f;
  float kz = kk * dot(d,a);

  float2 res;

  float p = ky - kx * kx;
  float p3 = p * p * p;
  float q = kx * (2.0f * kx * kx - 3.0f * ky) + kz;
  float h = q * q + 4.0f * p3;

  if(h >= 0.0f) 
  { 
      h = sqrt(h);
      float2 x = (float2(h, -h) - q) / 2.0f;
      float2 uv = sign(x) * pow(abs(x), 0.33333333f);
      float t = clamp(uv.x + uv.y - kx, 0.0f, 1.0f);

      // 1 root
      float3 g = d + (c + b * t) * t;
      res = float2(dot(g, g), t);
  }
  else
  {
      float z = sqrt(-p);
      float v = acos(q / (p * z * 2.0f)) / 3.0f;
      float m = cos(v);
      float n = sin(v) * 1.732050808f;
      float3 t = clamp(float3(m + m,-n - m, n - m) * z - kx, 0.0f, 1.0f);
      
      // 3 roots, but only need two
      float3 g = d + (c + b * t.x) * t.x;
      float dis = dot(g, g);
      res = float2(dis, t.x);

      g = d + (c + b * t.y) * t.y;
      dis = dot(g, g);
      if(dis < res.x)
        res = float2(dis, t.y);
  }
  
  res.x = sqrt(res.x);
  return res;
}

#endif
