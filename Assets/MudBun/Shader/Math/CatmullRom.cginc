/******************************************************************************/
/*
  Project   - MudBun
  Publisher - Long Bunny Labs
              http://LongBunnyLabs.com
  Author    - Ming-Lun "Allen" Chou
              http://AllenChou.net
*/
/******************************************************************************/

#ifndef MUDBUN_CATMULL_ROM
#define MUDBUN_CATMULL_ROM

float bezier_quad(float p0, float p1, float p2, float t)
{
  return lerp(lerp(p0, p1, t), lerp(p1, p2, t), t);
}

float2 bezier_quad(float2 p0, float2 p1, float2 p2, float t)
{
  return lerp(lerp(p0, p1, t), lerp(p1, p2, t), t);
}

float3 bezier_quad(float3 p0, float3 p1, float3 p2, float t)
{
  return lerp(lerp(p0, p1, t), lerp(p1, p2, t), t);
}

float4 bezier_quad(float4 p0, float4 p1, float4 p2, float t)
{
  return lerp(lerp(p0, p1, t), lerp(p1, p2, t), t);
}

float bezier_quad_grad(float p0, float p1, float p2, float t)
{
  float t2 = t + t;
  return p0 * (t2 - 2.0f) + p1 * (2.0f - t2 - t2) + p2 * t2;
}

float2 bezier_quad_grad(float2 p0, float2 p1, float2 p2, float2 t)
{
  float2 t2 = t + t;
  return p0 * (t2 - 2.0f) + p1 * (2.0f - t2 - t2) + p2 * t2;
}

float3 bezier_quad_grad(float3 p0, float3 p1, float3 p2, float3 t)
{
  float3 t2 = t + t;
  return p0 * (t2 - 2.0f) + p1 * (2.0f - t2 - t2) + p2 * t2;
}

float4 bezier_quad_grad(float4 p0, float4 p1, float4 p2, float4 t)
{
  float4 t2 = t + t;
  return p0 * (t2 - 2.0f) + p1 * (2.0f - t2 - t2) + p2 * t2;
}

float catmull_rom(float p0, float p1, float p2, float p3, float t)
{
  float tt = t * t;
  return 
    0.5f 
    * ((2.0f *  p1) 
        + (-p0 + p2) * t 
        + (2.0f *  p0 - 5.0f * p1 + 4.0f * p2 - p3) * tt 
        + (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * tt * t 
      );
}

float2 catmull_rom(float2 p0, float2 p1, float2 p2, float2 p3, float t)
{
  float tt = t * t;
  return 
    0.5f 
    * ((2.0f *  p1) 
        + (-p0 + p2) * t 
        + (2.0f *  p0 - 5.0f * p1 + 4.0f * p2 - p3) * tt 
        + (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * tt * t
      );
}

float3 catmull_rom(float3 p0, float3 p1, float3 p2, float3 p3, float t)
{
  float tt = t * t;
  return 
    0.5f 
    * ((2.0f *  p1) 
        + (-p0 + p2) * t 
        + (2.0f *  p0 - 5.0f * p1 + 4.0f * p2 - p3) * tt 
        + (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * tt * t
      );
}

float4 catmull_rom(float4 p0, float4 p1, float4 p2, float4 p3, float t)
{
  float tt = t * t;
  return 
    0.5f 
    * ((2.0f *  p1) 
        + (-p0 + p2) * t 
        + (2.0f *  p0 - 5.0f * p1 + 4.0f * p2 - p3) * tt 
        + (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * tt * t
      );
}

#endif
